# EE-RAG — Elective Ephemeral Retrieval-Augmented Generation

> Extracted from [AGPA](https://github.com/johnbrodowski) — a fully autonomous general-purpose agent (~150k LOC, closed-source). This library represents the RAG subsystem running in production.

EE-RAG is a .NET 10 RAG framework that solves three fundamental problems with naive RAG:

| Naive RAG problem | EE-RAG solution |
|---|---|
| Every retrieved chunk goes into the prompt whether relevant or not | Model **elects** which candidates it actually needs |
| Retrieved context pollutes conversation history permanently | Injected context is **ephemeral** — never stored in message history |
| Token costs scale with retrieval window | Only elected chunks consume tokens; surfaced candidates cost ~25 tokens each |

The result is 68–95% fewer retrieval tokens than naive RAG (depending on election rate), with full provenance tracking via hash-tagged audit trails.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [The Five Layers](#the-five-layers)
3. [Project Structure](#project-structure)
4. [Prerequisites & Installation](#prerequisites--installation)
5. [Configuration](#configuration)
6. [AI Provider Setup](#ai-provider-setup)
7. [Demo Application](#demo-application)
8. [QA Dataset Benchmark](#qa-dataset-benchmark)
   - [Importing a Dataset](#importing-a-dataset)
   - [Embedding Backfill](#embedding-backfill)
   - [Running a Benchmark](#running-a-benchmark)
   - [Score Thresholds](#score-thresholds)
   - [Real-Time Accuracy Monitor](#real-time-accuracy-monitor)
   - [Auto-Tune](#auto-tune)
   - [Reading the Report](#reading-the-report)
   - [Settings Persistence](#settings-persistence)
9. [EE-RAG Pipeline Benchmark](#ee-rag-pipeline-benchmark)
10. [Using the Library Directly](#using-the-library-directly)
11. [Running Tests](#running-tests)
12. [Troubleshooting](#troubleshooting)
13. [License](#license)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        DemoApp (WinForms)                    │
│   Form1 — EE-RAG chat demo                                   │
│   FormQaBenchmark — QA dataset benchmarking UI               │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│                     LocalRAG (core library)                  │
│                                                              │
│  EERagPipeline          ← orchestrates the 5-layer pipeline  │
│  EmbeddingDatabaseNew   ← SQLite + LSH + FTS5 + caching      │
│  EmbedderClassNew       ← BERT via ONNX Runtime              │
│  RAGConfiguration       ← all tuneable parameters            │
│                                                              │
│  Benchmarks/                                                 │
│    EERagBenchmark       ← pipeline-level F1/recall metrics   │
│                                                              │
│  QaDataset/                                                  │
│    QaBenchmarkRunner    ← model accuracy over JSONL datasets  │
│    QaDatasetDatabase    ← SQLite for Q&A items + results     │
│    QaEmbeddingBackfiller← background embedding worker        │
│    QaBenchmarkReport    ← formatted pass/fail reporting       │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│               AIClients / AiMessagingCore                    │
│                                                              │
│  AiSessionBuilder  ← fluent session factory                  │
│  IChatSession      ← provider-agnostic chat interface        │
│                                                              │
│  Providers: Anthropic · OpenAI · Groq · DeepSeek            │
│             Grok · LM Studio · LlamaSharp · Duck             │
└─────────────────────────────────────────────────────────────┘
```

---

## The Five Layers

EE-RAG processes every query through five coordinated layers:

### Layer 1 — Summary-first candidate surfacing
Instead of injecting full document chunks, the pipeline first retrieves only **~25-token summaries** for the top-k candidates. These are assembled into a candidate header and shown to the model alongside the user's query.

```
--- Potentially Relevant Context ---
[ID:1] Machine learning fundamentals and gradient descent overview
[ID:2] Neural network architecture and backpropagation
[ID:3] Supervised vs unsupervised learning comparison
```

Cost: ~25 tokens per candidate regardless of document size.

### Layer 2 — Model-elected context expansion
The model evaluates the summaries and responds with a `RETRIEVE` command for any it wants to read in full:

```
RETRIEVE 1 3
```

Only the IDs surfaced in Layer 1 are accepted (prompt injection protection). If the model doesn't need any candidates, the pipeline short-circuits — no retrieval occurs and no full chunks are consumed.

### Layer 3 — Transient injection with hash provenance
Elected chunks are fetched and injected as **transient background** — a special injection path that is used for inference but never stored in the session's persistent message history. Each chunk's BERT embedding is SHA-256 hashed to an 8-character hex identifier:

```
Final response: "The key idea behind gradient descent is... [RAG:a3f9b2c1] [RAG:7d4e1f83]"
```

The `[RAG:hash]` tags embedded in the final response serve as cryptographic references back to the exact embedding that contributed the knowledge — full provenance at 8 tokens total.

### Layer 4 — Conversation history isolation
The session's `.Messages` list never contains:
- Candidate summaries
- RETRIEVE requests
- Elected chunk content

It contains only clean turns: the user's original question and the assistant's final response (with hash tags). Context injection is invisible to future turns.

### Layer 5 — Silent digestion mode (optional)
When `SilentMode = true`, elected chunks are distilled into a compact synthesis before injection. The model receives a compressed summary rather than raw document text, further reducing token consumption for long documents.

---

## Project Structure

```
EE-RAG/
├── LocalRAG/                         # Core library (.NET 10)
│   ├── EERagPipeline.cs              # Main pipeline orchestrator
│   ├── EmbeddingDatabaseNew.cs       # SQLite embedding store
│   ├── EmbedderClassNew.cs           # BERT ONNX inference
│   ├── RAGConfiguration.cs           # Tuneable parameters
│   ├── Benchmarks/
│   │   ├── EERagBenchmark.cs         # Pipeline benchmark runner
│   │   ├── EERagBenchmarkModels.cs   # Dataset and report models
│   │   └── BenchmarkData/
│   │       └── eedag_benchmark_v1.json  # Embedded reference dataset
│   └── QaDataset/
│       ├── QaDatasetItem.cs          # Q&A item + outcome models
│       ├── QaDatasetDatabase.cs      # SQLite for Q&A items/results
│       ├── QaDatasetLoader.cs        # JSONL importer
│       ├── QaBenchmarkRunner.cs      # Benchmark execution engine
│       ├── QaBenchmarkReport.cs      # Report formatting + pass/fail
│       ├── BenchmarkProgressUpdate.cs # Real-time progress structure
│       └── QaEmbeddingBackfiller.cs  # Background embedding worker
│
├── AIClients/
│   ├── AiMessagingCore/              # Provider abstraction library
│   │   ├── Core/
│   │   │   ├── AiSessionBuilder.cs   # Fluent session factory
│   │   │   ├── AiSession.cs          # Session implementation
│   │   │   └── AiProviderFactory.cs  # Provider registry
│   │   ├── Configuration/
│   │   │   ├── AiSettings.cs         # Load/save ai-settings.json
│   │   │   ├── AiLibrarySettings.cs  # Root settings model
│   │   │   ├── ProviderSettings.cs   # Per-provider config
│   │   │   └── ModelDefaults.cs      # Temperature, maxTokens, etc.
│   │   └── Providers/
│   │       ├── Anthropic/
│   │       ├── OpenAI/
│   │       ├── Groq/
│   │       ├── DeepSeek/
│   │       ├── Grok/
│   │       ├── Local/                # LM Studio, LlamaSharp
│   │       └── Duck/
│   └── AIClients/                    # Standalone AI client app
│
├── DemoApp/                          # Windows Forms demo (.NET 10)
│   ├── Form1.cs / .Designer.cs       # EE-RAG pipeline demo
│   ├── FormQaBenchmark.cs / .Designer.cs  # QA benchmark UI
│   ├── BenchmarkSettings.cs          # Persisted benchmark config
│   └── Program.cs
│
└── LocalRAG.Tests/                   # Test suite
    ├── EERagBenchmarkTests.cs        # Pipeline benchmark tests
    ├── IntegrationTests.cs           # Embedding + search tests
    ├── CosineSimilarityTests.cs      # Vector math tests
    ├── MemoryHashIndexTests.cs       # LSH index tests
    ├── WordMatchScoreTests.cs        # Text scoring tests
    └── LSHTests.cs                   # LSH correctness tests
```

---

## Prerequisites & Installation

**Requirements:**
- .NET 10.0 SDK
- Windows (WinForms UI) or Windows/Linux/macOS (library only)
- A BERT model in ONNX format (see below)
- At least one LLM provider API key

**1. Clone and restore:**
```bash
git clone https://github.com/johnbrodowski/EE-RAG.git
cd EE-RAG
dotnet restore
```

**2. Download a BERT model:**

The recommended model is `all-MiniLM-L6-v2` (fast, 768-dim, ~90MB):
```
https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/tree/main/onnx
```

Download `model.onnx` and `vocab.txt`. Place them at:
```
EE-RAG/all-MiniLM-L6-v2-ONNX/model.onnx
EE-RAG/all-MiniLM-L6-v2-ONNX/vocab.txt
```

Or use any BERT-family ONNX model and point `RAGConfiguration.ModelPath` / `VocabularyPath` to it. Both 768-dim (base) and 1024-dim (large) models are supported.

**3. Configure your AI provider** — see [AI Provider Setup](#ai-provider-setup).

**4. Build:**
```bash
dotnet build LocalRAG.sln
```

---

## Configuration

`RAGConfiguration` controls the core pipeline behaviour:

```csharp
var config = new RAGConfiguration
{
    // Paths
    DatabasePath     = "Database/Memory/FeedbackEmbeddings512.db",
    ModelPath        = "all-MiniLM-L6-v2-ONNX/model.onnx",
    VocabularyPath   = "all-MiniLM-L6-v2-ONNX/vocab.txt",

    // Tokenisation / chunking
    MaxSequenceLength = 128,    // max BERT token window
    WordsPerString    = 40,     // words per chunk before embedding
    OverlapPercentage = 25,     // % overlap between adjacent chunks

    // LSH search index
    NumberOfHashFunctions = 8,  // hash functions per table
    NumberOfHashTables    = 10, // number of LSH tables (more = better recall, more RAM)

    // ONNX Runtime threading
    InterOpNumThreads = 32,     // parallelism across ops
    IntraOpNumThreads = 2,      // parallelism within a single op

    // In-memory embedding cache
    MaxCacheItems         = 10_000,
    CacheItemSizeThreshold = 1_048_576,  // 1 MB max per cache entry
    CacheExpiry           = TimeSpan.FromMinutes(15),

    // Async update queue
    MaxQueueSize     = 1000,
    MaxRetryAttempts = 3,
    RetryDelayMs     = 1000
};
```

**Defaults:** all values shown above are the defaults. You only need to set the three path properties for a minimal working setup.

---

## AI Provider Setup

Providers are configured in `ai-settings.json` (created automatically next to the executable on first run):

```json
{
  "DefaultProvider": "Anthropic",
  "TimeoutSeconds": 120,
  "Providers": {
    "Anthropic": {
      "ProviderType": "Anthropic",
      "ApiKey": "sk-ant-YOUR_KEY",
      "BaseUrl": "https://api.anthropic.com/v1",
      "StreamingEnabled": true,
      "Defaults": {
        "Model": "claude-sonnet-4-6",
        "Temperature": 0.7,
        "MaxTokens": 4096
      }
    },
    "OpenAI": {
      "ProviderType": "OpenAI",
      "ApiKey": "sk-YOUR_KEY",
      "Defaults": {
        "Model": "gpt-4o",
        "Temperature": 0.7,
        "MaxTokens": 4096
      }
    },
    "Groq": {
      "ProviderType": "Groq",
      "ApiKey": "gsk_YOUR_KEY",
      "Defaults": {
        "Model": "llama-3.3-70b-versatile",
        "Temperature": 0.7,
        "MaxTokens": 4096
      }
    },
    "LMStudio": {
      "ProviderType": "LMStudio",
      "BaseUrl": "http://localhost:1234/v1",
      "Defaults": {
        "Model": "local-model",
        "Temperature": 0.7
      }
    }
  }
}
```

API keys can also be supplied via environment variables:

| Provider | Environment variable |
|---|---|
| Anthropic | `ANTHROPIC_API_KEY` |
| OpenAI | `OPENAI_API_KEY` |
| Groq | `GROQ_API_KEY` |
| DeepSeek | `DEEPSEEK_API_KEY` |
| Grok (xAI) | `XAI_API_KEY` |
| LM Studio | `LMSTUDIO_BASE_URL` |

Environment variables override values in `ai-settings.json`.

### Using the session builder

```csharp
var session = AiSessionBuilder
    .WithProvider("Anthropic")
    .WithModel("claude-haiku-4-5-20251001")
    .WithTemperature(0.3)
    .WithMaxTokens(1024)
    .WithSystemMessage("You are a factual assistant.")
    .Build();

var reply = await session.SendAsync("What is gradient descent?");
Console.WriteLine(reply.Content);
```

---

## Demo Application

Launch from the `DemoApp` project:
```bash
cd DemoApp
dotnet run
```

The main form (`Form1`) has two panels:

### Semantic Search panel
Search the embedding database directly. Enter a query and click **Search** to retrieve the top-30 semantically similar documents ranked by cosine similarity. Results show the stored request, response, and similarity score.

### EE-RAG Pipeline panel
Run a full EE-RAG query:

1. Type a question in the query box.
2. Select a **Provider** and **Model** from the dropdowns.
3. Click **Ask AI**.

The results pane displays the full pipeline trace:
```
[Query]      What is backpropagation?

[Candidates] --- Potentially Relevant Context ---
             [ID:2] Neural network architecture and backpropagation
             [ID:5] Gradient flow and vanishing gradients

[Phase 1]    RETRIEVE 2 5

[Retrieved]  Elected IDs: 2, 5
             Hashes: a3f9b2c1, 7d4e1f83

[Final]      Backpropagation is the algorithm used to train neural networks by
             computing gradients of the loss function with respect to weights
             using the chain rule... [RAG:a3f9b2c1] [RAG:7d4e1f83]
```

The conversation history stored in the database will only contain the user's question and the final response. The candidate summaries and retrieved chunks never appear in the persistent history.

---

## QA Dataset Benchmark

The QA benchmark evaluates a model's factual accuracy against a ground-truth Q&A dataset in JSONL format (compatible with the TriviaQA format). Open it from the main application's menu or run `FormQaBenchmark` directly.

### Dataset Format

Each line in the JSONL file is a JSON object:
```json
{
  "question": "What is the capital of France?",
  "answer": ["Paris"],
  "def_correct_predictions": ["paris", "paris, france"],
  "poss_correct_predictions": ["french capital", "île-de-france"],
  "def_incorrect_predictions": ["london", "berlin", "madrid"],
  "answer_and_def_correct_predictions": ["paris", "paris, france"]
}
```

| Field | Meaning |
|---|---|
| `answer` | Ground-truth answers |
| `def_correct_predictions` | Strings whose presence in the response → **Correct** |
| `poss_correct_predictions` | Strings whose presence → **Possibly Correct** |
| `def_incorrect_predictions` | Strings whose presence → **Definitely Wrong** |
| `answer_and_def_correct_predictions` | Combined correct set (most commonly used) |

Evaluation priority: Correct → Definitely Wrong → Possibly Correct → Indeterminate (no match).

### Importing a Dataset

1. Click **Browse…** and select your `.jsonl` file.
2. Click **Import**.

The importer reads the file lazily and inserts records in batches of 500, reporting progress in the status bar. Large datasets (50k+ questions) import in under a minute.

The import is idempotent — re-importing the same file will add new items and skip duplicates based on the question text.

### Embedding Backfill

The **RAG context injection** feature (which injects semantically similar Q&A pairs as context for each test question) requires questions to be embedded with BERT. After import:

1. Ensure your BERT model is configured (see [Configuration](#configuration)).
2. Click **Start Backfill**.

The backfiller runs in the background and embeds questions incrementally. You can click **Stop** at any time and resume later — it continues from where it left off. Progress is shown as `Embedded: X / Y`.

Backfill is optional. If embeddings are not available, RAG context injection is silently disabled for unembedded items.

### Running a Benchmark

Configure the run in the **Run Benchmark** group:

| Control | Purpose |
|---|---|
| **Provider** | LLM provider (Anthropic, OpenAI, Groq, DeepSeek, Grok) |
| **Model** | Model identifier, e.g. `claude-haiku-4-5-20251001` |
| **Temp** | Sampling temperature (0.00–2.00) |
| **Questions** | How many questions to test (randomly sampled) |
| **Inject similar Q&A pairs** | Enable RAG context injection (requires embeddings) |

Click **Run**. While the benchmark runs:
- Each question and its outcome is logged to the results pane.
- The live stats bar (bottom of the Run group) updates after every answer.
- Click **Run** again at any point to cancel.

When complete, click **Save Report…** to export the full per-question report as a `.txt` file.

### Score Thresholds

The **Score Thresholds** group lets you define pass/fail targets for each outcome category:

| Threshold | Direction | Default | Meaning |
|---|---|---|---|
| **Correct ≥** | at least | 50% | Run passes if ≥50% answers are Correct |
| **Poss. Correct ≥** | at least | 10% | Run passes if ≥10% answers are Possibly Correct |
| **Def. Wrong ≤** | at most | 10% | Run passes if ≤10% answers are Definitely Wrong |
| **Indeterm. ≤** | at most | 30% | Run passes if ≤30% answers are Indeterminate |

Thresholds appear in the saved report next to each metric:

```
Correct           :    14 (28.0%)  [FAIL ≥50%]
Possibly Correct  :     2 (4.0%)   [FAIL ≥10%]
Definitely Wrong  :     3 (6.0%)   [PASS ≤10%]
Indeterminate     :    31 (62.0%)  [FAIL ≤30%]
```

### Real-Time Accuracy Monitor

The live stats bar in the Run Benchmark group updates after each question is answered:

```
Q: 23/50  ✓26%  ?9%  ✗11%  –54%
```

During Auto-Tune runs the bar shows the current combination number and its running accuracy:

```
[Tune 4/12]  Q: 7/10  ✓40%  ?10%  ✗10%  –40%
```

### Auto-Tune

Auto-Tune sweeps a grid of model × temperature combinations to find the configuration that maximises your chosen score metric. Configure it in the **Auto-Tune** group:

| Control | Purpose |
|---|---|
| **Temperatures** | Comma-separated values to try, e.g. `0.0, 0.3, 0.5, 0.7, 1.0, 1.5` |
| **Models (csv)** | Comma-separated model names to try; **leave blank** to use only the current model |
| **Qs/run** | Questions per mini-run (10–20 is good for speed; 50+ for reliability) |
| **Score by** | Metric to optimise (see below) |

**Scoring options:**

| Option | Formula |
|---|---|
| Correct % | `CorrectRate` |
| Correct + Possibly Correct % | `CorrectRate + PossiblyCorrectRate` |
| Composite | `CorrectRate + 0.5 × PossiblyCorrectRate − 2 × DefinitelyWrongRate` |

Click **Auto-Tune**. The results pane shows a line per combination, flagging each new best:

```
=== Auto-Tune: 6 combinations × 10 questions each ===
    Temperatures : 0.00, 0.30, 0.50, 0.70, 1.00, 1.50
    Models       : claude-haiku-4-5-20251001
    Score metric : Correct %

  [1/6] model=claude-haiku-4-5-20251001  temp=0.00
         ✓20.0%  ?0.0%  ✗10.0%  –70.0%  score=20.00
  [2/6] model=claude-haiku-4-5-20251001  temp=0.30
         ✓30.0%  ?0.0%  ✗10.0%  –60.0%  score=30.00 ← new best
  [3/6] model=claude-haiku-4-5-20251001  temp=0.50
         ✓40.0%  ?10.0%  ✗0.0%  –50.0%  score=40.00 ← new best
  ...

=== Auto-Tune complete ===
    Best config  : model=claude-haiku-4-5-20251001  temp=0.50  score=40.00
```

On completion, the best model and temperature are automatically applied to the main **Run Benchmark** controls so you can immediately run a full test with the optimal settings.

Click **Stop** to cancel after the current mini-run finishes.

### Reading the Report

The report header shows run metadata:

```
=== QA Benchmark Report (RunId: A1B2C3D4) ===
Generated : 2026-03-09 14:22:10
Duration  : 03:47.214
Provider  : Anthropic
Model     : claude-haiku-4-5-20251001
Temperature: 0.50
```

Followed by outcome summary with pass/fail indicators (if thresholds are set):

```
Questions tested  : 50
Correct           :    20 (40.0%)  [FAIL ≥50%]
Possibly Correct  :     5 (10.0%)  [PASS ≥10%]
Definitely Wrong  :     4 (8.0%)   [PASS ≤10%]
Indeterminate     :    21 (42.0%)  [FAIL ≤30%]
```

Followed by a per-question breakdown:

```
─── Per-Question Results ───────────────────────────────────────────

[+] Who wrote Hamlet?
    Matched : "shakespeare"
    Response: Hamlet was written by William Shakespeare...

[?] What year was the Eiffel Tower completed?
    Matched : "1889"
    Response: The Eiffel Tower was completed in 1889...

[X] What is the chemical symbol for gold?
    Matched : "au"
    Response: The chemical symbol for gold is Fe...

[-] Who was the 31st president of the United States?
    Response: Herbert Hoover was the 31st president...
```

Icons: `[+]` Correct · `[?]` Possibly Correct · `[X]` Definitely Wrong · `[-]` Indeterminate

### Settings Persistence

All benchmark settings are automatically saved to `benchmark-settings.json` in the application directory every time you click **Run** or **Auto-Tune**. They are restored on next launch.

Saved settings include: provider, model, temperature, question count, RAG toggle, all four thresholds, and all four Auto-Tune fields (temperatures list, models list, Qs/run, score metric).

You can also save settings explicitly at any time with the **Save Settings** button in the Thresholds group.

---

## EE-RAG Pipeline Benchmark

The `EERagBenchmark` class provides a separate benchmark that measures the **pipeline itself** — not model accuracy, but retrieval quality. It answers: "Does the model elect the right documents?"

The benchmark uses the embedded reference dataset `eedag_benchmark_v1.json` which contains:
- **Knowledge entries**: Q&A pairs with summaries, seeded into the database
- **Benchmark cases**: Test queries with expected retrieval slugs and answer hints

### Metrics

| Metric | Definition |
|---|---|
| **Candidate Recall** | % of expected documents that appeared in the top-k candidate list |
| **Retrieval Precision** | % of elected documents that were expected |
| **Retrieval Recall** | % of expected documents that were elected |
| **F1** | Harmonic mean of precision and recall |
| **Retrieval Rate** | % of queries that triggered any retrieval |

### Running programmatically

```csharp
using LocalRAG.Benchmarks;

// Load the embedded reference dataset
var dataset = EERagBenchmark.LoadEmbeddedDataset();

// Seed the knowledge base
using var db = new EmbeddingDatabaseNew(config);
var slugToId = await EERagBenchmark.SeedDatabaseAsync(db, dataset);

// Run the benchmark
var report = await EERagBenchmark.RunAsync(
    dataset,
    sessionFactory: provider => AiSessionBuilder.WithProvider(provider).WithModel("claude-haiku-4-5-20251001").Build(),
    opts: new BenchmarkOptions
    {
        TopK       = 5,
        SilentMode = false
    });

Console.WriteLine($"Mean F1:           {report.MeanF1:F3}");
Console.WriteLine($"Candidate Recall:  {report.MeanCandidateRecall:F3}");
Console.WriteLine($"Retrieval Recall:  {report.MeanRetrievalRecall:F3}");
Console.WriteLine($"Retrieval Rate:    {report.RetrievalRateActual:F3}");
```

### Interpreting results

- **Candidate Recall ≥ 0.8**: The LSH + FTS search is surfacing the right documents in the top-k list. If low, increase `TopK` in `RAGConfiguration`.
- **F1 ≥ 0.5**: The model is electing relevant documents at an acceptable rate. The production benchmark expects F1 > 0.5 for the integration test to pass.
- **Retrieval Rate ≈ Expected Rate**: The model is electing to retrieve roughly as often as the dataset expects. A much lower actual rate suggests the summaries aren't compelling enough; a much higher rate suggests the model is over-retrieving.

---

## Using the Library Directly

### Add a document and search

```csharp
using LocalRAG;

var config = new RAGConfiguration
{
    ModelPath      = "path/to/model.onnx",
    VocabularyPath = "path/to/vocab.txt",
    DatabasePath   = "path/to/embeddings.db"
};

await using var db = new EmbeddingDatabaseNew(config);

// Store a request + response pair
await db.AddRequestToEmbeddingDatabaseAsync(
    requestId: "entry-001",
    theRequest: "What is backpropagation?",
    embed: true);

await db.UpdateTextResponse(
    requestId: "entry-001",
    message: "Backpropagation computes gradients via the chain rule...",
    embed: true);

await db.UpdateSummary(
    requestId: "entry-001",
    summary: "Backpropagation: gradient computation via chain rule");

// Search
var results = await db.SearchEmbeddingsAsync(
    searchText: "how do neural networks learn?",
    topK: 5,
    minimumSimilarity: 0.6f);

foreach (var r in results)
    Console.WriteLine($"{r.Similarity:F3}  {r.Summary}");
```

### Run the EE-RAG pipeline

```csharp
using LocalRAG;
using AiMessagingCore.Core;

var config  = new RAGConfiguration { /* ... */ };
var db      = new EmbeddingDatabaseNew(config);
var session = AiSessionBuilder
    .WithProvider("Anthropic")
    .WithModel("claude-haiku-4-5-20251001")
    .WithMaxTokens(2048)
    .WithSystemMessage(EERagPipeline.DefaultSystemPrompt)
    .Build();

var pipeline = new EERagPipeline(db, session);

var result = await pipeline.RunPipelineAsync(
    requestId: Guid.NewGuid().ToString("N")[..8],
    userMessage: "What are the key principles of gradient descent?",
    topK: 5,
    silentMode: false);

Console.WriteLine(result.FinalResponse);
Console.WriteLine($"Retrieved: {result.RetrievalOccurred}");
Console.WriteLine($"Elected IDs: {string.Join(", ", result.ElectedIds)}");
Console.WriteLine($"Hashes: {string.Join(", ", result.ElectedHashes)}");
```

### Switching providers mid-conversation

```csharp
// Switch the underlying provider without losing conversation history
await session.SwitchProviderAsync("OpenAI");

// Switch model within the same provider
await session.SwitchModelAsync("gpt-4o");
```

---

## Running Tests

```bash
# Fast unit tests only (no BERT model required)
dotnet test --filter "Category!=Integration"

# All tests including integration (requires BERT model)
cp test.runsettings.example test.runsettings
# Edit test.runsettings: set BERT_MODEL_PATH and BERT_VOCAB_PATH
dotnet test --settings test.runsettings
```

**test.runsettings** minimal content:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <TestRunParameters>
    <Parameter name="BERT_MODEL_PATH" value="C:\absolute\path\to\model.onnx" />
    <Parameter name="BERT_VOCAB_PATH"  value="C:\absolute\path\to\vocab.txt"  />
  </TestRunParameters>
</RunSettings>
```

### Test categories

| Suite | Tests | Requires |
|---|---|---|
| `CosineSimilarityTests` | Vector math | Nothing |
| `MemoryHashIndexTests` | LSH index | Nothing |
| `WordMatchScoreTests` | Text scoring | Nothing |
| `LSHTests` | LSH correctness | Nothing |
| `EERagBenchmarkTests` (unit) | Pipeline parsing, hashing, formatting | Nothing |
| `IntegrationTests` | BERT embeddings, similarity search | BERT model |
| `EERagBenchmarkTests` (AI) | Full pipeline F1 | BERT model + API key |

Expected output (unit tests only):
```
Test summary: total: 33, failed: 0, succeeded: 33, skipped: 0, duration: 2.1s
```

Full suite including integration:
```
Test summary: total: 38, failed: 0, succeeded: 38, skipped: 0, duration: 13.4s
```

---

## Troubleshooting

### BERT model not found
Ensure `RAGConfiguration.ModelPath` points to the `.onnx` file directly (not the directory). Use absolute paths if relative paths aren't resolving.

### Low candidate recall in the pipeline benchmark
- Increase `NumberOfHashTables` (default 10) for better LSH coverage at the cost of more RAM.
- Increase `TopK` to surface more candidates per query.
- Check that summaries are descriptive — they drive the first-phase retrieval.

### High indeterminate rate in QA benchmark
Indeterminate means no string from any prediction list was found in the model response. Common causes:
- The model is giving longer, rephrased answers that don't contain the exact substrings.
- Temperature is too high, producing inconsistent response formats.
- The dataset's prediction lists are sparse.

Try: lowering temperature, enabling RAG context injection (similar Q&A pairs guide the response format), or running Auto-Tune to find the configuration that produces the most matchable answers.

### Out of memory during backfill
Reduce `MaxCacheItems` in `RAGConfiguration`. The backfiller processes items one at a time but the cache retains embeddings. 5,000–10,000 is a reasonable range for most machines.

### Slow ONNX inference
- Use a smaller model (all-MiniLM-L6-v2 is significantly faster than BERT-large).
- Tune `InterOpNumThreads` to match your physical core count.
- For very large backfill jobs, set `IntraOpNumThreads = 1` and `InterOpNumThreads = (core count)` for better throughput.

### API timeout during long benchmarks
Increase `TimeoutSeconds` in `ai-settings.json`. For long Auto-Tune sweeps over many combinations, 180–300 seconds is recommended.

---

## License

Apache License 2.0 — see [LICENSE.txt](LICENSE.txt) for details.

BERT models are distributed separately under their own licences (typically Apache 2.0 or MIT). See [Hugging Face](https://huggingface.co/models?library=onnx&search=bert) for model-specific terms.

---

## Acknowledgments

- [ONNX Runtime](https://github.com/microsoft/onnxruntime) — BERT inference
- [FastBertTokenizer](https://github.com/NMZivkovic/FastBertTokenizer) — tokenisation
- [Microsoft.Data.Sqlite](https://github.com/dotnet/efcore) — embedded database
- BERT models from [Hugging Face](https://huggingface.co/)
