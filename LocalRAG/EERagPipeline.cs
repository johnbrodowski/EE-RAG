using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using AiMessagingCore.Abstractions;

namespace LocalRAG
{
    /// <summary>
    /// Result returned by <see cref="EERagPipeline.RunPipelineAsync"/>.
    /// </summary>
    public sealed class EERagResult
    {
        /// <summary>The final assistant response text (including any [RAG:hash] tags).</summary>
        public string FinalResponse { get; init; } = string.Empty;

        /// <summary>The candidate summary block that was sent as transient context in Phase 1.</summary>
        public string CandidateHeader { get; init; } = string.Empty;

        /// <summary>The raw first response from the model (may contain a RETRIEVE command).</summary>
        public string FirstResponse { get; init; } = string.Empty;

        /// <summary>IDs of the candidates that were surfaced to the model (Phase 1 audit).</summary>
        public List<int> CandidateIdsSurfaced { get; init; } = [];

        /// <summary>IDs elected by the model via RETRIEVE command (empty if none requested).</summary>
        public List<int> ElectedIds { get; init; } = [];

        /// <summary>8-hex hashes of the elected chunks appended to the assistant turn (Phase 2 audit).</summary>
        public List<string> ElectedHashes { get; init; } = [];

        /// <summary>True if the model elected at least one chunk for full retrieval.</summary>
        public bool RetrievalOccurred => ElectedIds.Count > 0;
    }

    /// <summary>
    /// Elective Ephemeral RAG pipeline — implements the five-layer EE-RAG architecture.
    ///
    /// Layer 1  Summary-first retrieval with candidate header.
    /// Layer 2  Model-elected context expansion (RETRIEVE command).
    /// Layer 3  Transient injection with 8-hex embedding hash references.
    /// Sec 3.4  Two-phase audit (rag_candidates_surfaced / rag_entries_elected).
    /// Layer 4  Conversation history isolation (RAG content never enters _messages).
    /// Layer 5  Silent digestion mode (optional — synthesises chunks before injection).
    /// </summary>
    public static class EERagPipeline
    {
        // ── Public entry point ───────────────────────────────────────────────

        /// <summary>
        /// Runs the full EE-RAG pipeline for a single user turn.
        /// </summary>
        /// <param name="userMessage">The user's message for this turn.</param>
        /// <param name="requestId">
        ///   The RequestID under which this turn will be stored in the knowledge base.
        ///   Used to write the two-phase audit metadata.
        /// </param>
        /// <param name="session">A live <see cref="IChatSession"/> that carries persistent conversation history.</param>
        /// <param name="db">The embedding database used for retrieval and audit writes.</param>
        /// <param name="topK">Number of candidate entries to retrieve.</param>
        /// <param name="silentMode">
        ///   When true (Layer 5), elected chunks are distilled into a compact synthesis before
        ///   injection so that no raw chunk text ever enters the inference context.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task<EERagResult> RunPipelineAsync(
            string userMessage,
            string requestId,
            IChatSession session,
            EmbeddingDatabaseNew db,
            int topK = 5,
            bool silentMode = false,
            CancellationToken cancellationToken = default)
        {
            // ── Step 1: Initial retrieval ────────────────────────────────────
            var candidates = await db.SearchEmbeddingsAsync(
                searchText: userMessage,
                topK: topK,
                minimumSimilarity: 0.00f,
                searchLevel: 2);

            // ── Step 2: Build summary list and candidate header ──────────────
            var candidateIds = candidates.Select(c => c.Id).ToList();
            var candidateHeader = BuildCandidateHeader(candidates);

            // ── Step 3: Phase 1 audit — write candidate IDs before inference ─
            await db.UpdateRagCandidatesSurfacedAsync(requestId, candidateIds);

            // ── Step 4 & 5: First inference — model evaluates summaries ──────
            // The candidate header is transient: provided for this call only,
            // never stored in the session's persistent Messages history.
            var firstMsg = await session.SendWithTransientBackgroundAsync(
                userMessage,
                candidateHeader,
                cancellationToken: cancellationToken);

            var firstResponse = firstMsg.Content;

            // ── Step 6: Parse RETRIEVE command ───────────────────────────────
            // Constrain to surfaced candidates — prevents a malformed or prompt-injected
            // model response from fetching unrelated rows by arbitrary ID.
            var electedIds = ParseRetrieveIds(firstResponse)
                .Where(id => candidateIds.Contains(id))
                .ToList();

            string finalResponse;
            var electedHashes = new List<string>();

            if (electedIds.Count == 0)
            {
                // No retrieval requested — first response is the final response.
                finalResponse = firstResponse;
            }
            else
            {
                // Phase-1 added a (user, RETRIEVE) pair to persistent history.
                // Remove it so only the final answer turn survives.
                session.TrimLastTurn();

                // ── Step 7: Fetch elected chunks in parallel ─────────────────
                var chunkResults = await Task.WhenAll(
                    electedIds.Select(id => db.GetFeedbackDataByIdAsync(id)));
                var electedChunks = chunkResults
                    .Where(c => c != null)
                    .Cast<FeedbackDatabaseValues>()
                    .ToList();

                // Compute 8-hex hash for each elected chunk.
                foreach (var chunk in electedChunks)
                {
                    var embedding = chunk.RequestEmbedding ?? chunk.SummaryEmbedding;
                    if (embedding != null)
                        electedHashes.Add(GenerateEmbeddingHash(embedding));
                }

                // Build the transient context for the second inference call.
                string transientContext;
                if (silentMode)
                {
                    // Layer 5: silent digestion — distil chunks; raw content never enters context.
                    transientContext = SynthesizeChunks(electedChunks);
                }
                else
                {
                    // Standard ephemeral injection — full chunk text as transient background.
                    // Summaries are omitted here; the model already evaluated them in Step 5.
                    transientContext = FormatFullChunks(electedChunks);
                }

                // ── Second inference with elected chunks (transient) ─────────
                var finalMsg = await session.SendWithTransientBackgroundAsync(
                    userMessage,
                    transientContext,
                    cancellationToken: cancellationToken);

                // ── Step 7: Append [RAG:hash] tags to assistant turn ─────────
                // Tags are ~5 tokens each. They carry no semantic content so cannot
                // confuse future turns or pollute the embedding space if re-indexed.
                var hashTags = string.Concat(electedHashes.Select(h => $" [RAG:{h}]"));
                finalResponse = finalMsg.Content + hashTags;
            }

            // ── Step 9: Phase 2 audit — record elected hashes post-inference ─
            await db.UpdateRagEntriesElectedAsync(requestId, electedHashes);

            return new EERagResult
            {
                FinalResponse        = finalResponse,
                CandidateHeader      = candidateHeader,
                FirstResponse        = firstResponse,
                CandidateIdsSurfaced = candidateIds,
                ElectedIds           = electedIds,
                ElectedHashes        = electedHashes
            };
        }

        // ── Utility: candidate header (Layer 1) ─────────────────────────────

        /// <summary>
        /// Builds the "--- Potentially Relevant Context ---" block defined in Section 3.1.
        /// Each entry shows only a ~25-token summary so the model can judge relevance
        /// without being flooded with full chunk content.
        /// Falls back to the first 150 characters of Request when Summary is absent.
        /// </summary>
        public static string BuildCandidateHeader(List<FeedbackDatabaseValues> candidates)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Potentially Relevant Context ---");
            sb.AppendLine("Each entry below has a DATABASE ID shown as [ID:<number>].");
            sb.AppendLine("To retrieve an entry's full content, respond with EXACTLY: RETRIEVE <number>");
            sb.AppendLine("You MUST use the exact number shown in [ID:<number>] — do not renumber the entries yourself.");
            sb.AppendLine("For multiple entries: RETRIEVE <id1> <id2> (space-separated).");
            sb.AppendLine("You are not required to retrieve any entry.");
            sb.AppendLine("---");

            foreach (var c in candidates)
            {
                // Prefer the pre-generated Summary field; fall back to the first ~150 chars of Request.
                var summaryText = !string.IsNullOrWhiteSpace(c.Summary)
                    ? c.Summary.Trim()
                    : TruncateToTokenBudget(c.Request ?? string.Empty, 150);

                sb.AppendLine($"[ID:{c.Id}] {summaryText}");
            }

            return sb.ToString();
        }

        // ── Utility: RETRIEVE command parser (Layer 2) ───────────────────────

        /// <summary>
        /// Extracts integer IDs from a "RETRIEVE n [n ...]" command in the model's response.
        /// Handles: "RETRIEVE 7", "RETRIEVE 3 7 12", case-insensitive, any surrounding text.
        /// Returns an empty list if no RETRIEVE command is present.
        /// </summary>
        public static List<int> ParseRetrieveIds(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return [];

            // Primary: RETRIEVE followed by one or more integers (space or comma separated).
            var match = Regex.Match(
                response,
                @"RETRIEVE\s+([\d\s,]+)",
                RegexOptions.IgnoreCase);

            if (match.Success)
                return Regex.Matches(match.Groups[1].Value, @"\d+")
                    .Select(m => int.Parse(m.Value))
                    .Distinct()
                    .ToList();

            // Fallback: model described an entry as relevant using [ID:N] notation but didn't
            // use the RETRIEVE keyword.  Only activate when the response clearly indicates
            // intent to retrieve (mentions "relevant", "retrieve", "most relevant", etc.).
            bool indicatesRetrieval = Regex.IsMatch(
                response,
                @"\b(relevant|retrieve|elect|useful|helpful|applies|related)\b",
                RegexOptions.IgnoreCase);

            if (indicatesRetrieval)
            {
                var ids = Regex.Matches(response, @"\[ID:(\d+)\]", RegexOptions.IgnoreCase)
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .Distinct()
                    .ToList();
                if (ids.Count > 0)
                    return ids;
            }

            return [];
        }

        // ── Utility: embedding hash (Layer 3) ────────────────────────────────

        /// <summary>
        /// Computes the 8-character hexadecimal hash described in Section 3.3.
        /// Uses SHA-256 over the raw embedding bytes; takes the first 4 bytes → 8 hex chars.
        /// The same embedding always produces the same hash, making it a deterministic
        /// audit key that maps back to the exact document in the index.
        /// </summary>
        public static string GenerateEmbeddingHash(float[] embedding)
        {
            var bytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);

            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash)[..8].ToLowerInvariant();
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Formats elected chunks as verbatim transient background context (standard mode).
        /// Summaries are excluded here — the model already evaluated them in Phase 1.
        /// </summary>
        private static string FormatFullChunks(List<FeedbackDatabaseValues> chunks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Retrieved Context (transient — not stored in conversation history) ---");

            foreach (var chunk in chunks)
            {
                sb.AppendLine($"Entry {chunk.Id}:");
                if (!string.IsNullOrWhiteSpace(chunk.Request))
                    sb.AppendLine($"  Request: {chunk.Request}");
                if (!string.IsNullOrWhiteSpace(chunk.TextResponse))
                    sb.AppendLine($"  Response: {chunk.TextResponse}");
                if (!string.IsNullOrWhiteSpace(chunk.ToolUseTextResponse))
                    sb.AppendLine($"  Tool Response: {chunk.ToolUseTextResponse}");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            return sb.ToString();
        }

        /// <summary>
        /// Layer 5: silent digestion — produces a compact distillation of elected chunks.
        /// No raw chunk content appears in the inference context; only the synthesis is injected.
        /// The synthesis is used once and discarded (not cached or persisted).
        /// </summary>
        private static string SynthesizeChunks(List<FeedbackDatabaseValues> chunks)
        {
            // Distil each chunk to its most essential fields at reduced length.
            var sb = new StringBuilder();
            sb.AppendLine("--- Background Knowledge (synthesised, transient) ---");

            foreach (var chunk in chunks)
            {
                var keyFacts = new List<string>();

                if (!string.IsNullOrWhiteSpace(chunk.Summary))
                    keyFacts.Add(chunk.Summary.Trim());
                else if (!string.IsNullOrWhiteSpace(chunk.Request))
                    keyFacts.Add(TruncateToTokenBudget(chunk.Request, 100));

                if (!string.IsNullOrWhiteSpace(chunk.TextResponse))
                    keyFacts.Add(TruncateToTokenBudget(chunk.TextResponse, 200));

                sb.AppendLine(string.Join(" | ", keyFacts));
            }

            sb.AppendLine("---");
            return sb.ToString();
        }

        private static string TruncateToTokenBudget(string text, int charBudget)
        {
            text = text.Trim();
            return text.Length <= charBudget ? text : text[..charBudget].TrimEnd() + "…";
        }
    }
}
