# EE-RAG

EE-RAG is a C#/.NET solution that implements **Elective Ephemeral RAG** with a WinForms demo UI, a reusable LocalRAG library, and a provider-agnostic AI messaging core.

This README is based on a systematic code review that started with `DemoApp/FormQaBenchmark.cs` and `DemoApp/Form1.cs`, then followed their call graph into `LocalRAG` and `AiMessagingCore`.

## What the system does

- Runs interactive EE-RAG chat through `Form1` using:
  - retrieval from an embeddings SQLite DB,
  - model-elected expansion using `RETRIEVE <id>` style behavior,
  - transient background injection (not persisted in chat history), and
  - per-turn audit metadata. 
- Runs two benchmark modes:
  - EE-RAG benchmark (knowledge/case dataset in `LocalRAG/BenchmarkData/eedag_benchmark_v1.json`).
  - QA dataset benchmark UI (`FormQaBenchmark`) with import, embedding backfill, run, autotune, and report save.

## Solution structure

- `DemoApp/`
  - Main WinForms app (`Form1`) for chat, EE-RAG benchmark, mock data generation, tests, and embedding backfill.
  - QA benchmark form (`FormQaBenchmark`) dedicated to JSONL QA datasets and model parameter tuning.
- `LocalRAG/`
  - `EERagPipeline`: core multi-step EE-RAG orchestration.
  - `EmbeddingDatabaseNew`: embeddings store, retrieval/search, metadata updates, and utilities.
  - `QaDataset/*`: QA dataset DB, loader, backfiller, benchmark runner/report models.
  - `Benchmarks/*`: EE-RAG benchmark dataset runner.
- `AIClients/`
  - Provider abstraction + concrete provider clients (OpenAI, Anthropic, Groq, etc.) and chat session infrastructure.

## Key runtime flows

### 1) Interactive EE-RAG ask flow

1. User enters query in `Form1` and clicks **Ask AI**.
2. App creates/uses AI session from provider + model selection.
3. App writes request row into embedding DB (pre-inference anchor).
4. `EERagPipeline.RunPipelineAsync` executes:
   - retrieve top candidates,
   - build candidate header,
   - write `rag_candidates_surfaced`,
   - model first pass,
   - parse elected IDs,
   - optional second pass with transient context,
   - append `[RAG:hash]` tags,
   - write `rag_entries_elected`.
5. UI prints a full pipeline trace.

### 2) QA dataset benchmark flow

1. `FormQaBenchmark` opens a dedicated SQLite DB under:
   - `Database/QaDataset/QaBenchmark.db` (under app base directory).
2. User imports JSONL dataset via `QaDatasetLoader`.
3. Optional backfill computes question embeddings for not-yet-embedded items.
4. `QaBenchmarkRunner` samples items, optionally injects similar QA examples, queries model, classifies outcomes, persists per-question run result.
5. UI prints run summary and can save to `.txt`.
6. Auto-Tune runs sweep combinations across models/temperatures and applies best config back to controls.

## Design observations from review

### Strengths

- Strong separation between UI orchestration and benchmark/pipeline library logic.
- Defensive flow in forms (input checks, cancellation tokens, UI state lock/unlock, exception handling).
- Auditability is first-class in EE-RAG pipeline (`candidate surfaced` and `entries elected`).
- QA benchmark pipeline supports repeatable tuning loops with configurable scoring formulas.

### Notable behavioral details

- QA evaluation is substring-based matching against configured prediction lists (simple and deterministic, but sensitive to phrasing variance).
- In QA benchmark runs, embedded items are prioritized, then unembedded items are used as fallback to satisfy `MaxQuestions`.
- Auto-tune uses the same provider across all model/temperature combinations in a sweep.
- Backfill stop is cooperative (stops after current item).

## Prerequisites

- Windows-capable .NET SDK that can build WinForms target `net10.0-windows7.0`.
- Local model files/config for embeddings (`RAGConfiguration`/`EmbedderClassNew`) if embedding generation is required.
- AI provider configuration file `ai-settings.json` (expected by demo flows).

See `AIClients/Docs/Examples/ai-settings.example.json` for template guidance.

## Build and run

Typical local workflow (Windows):

1. Create `ai-settings.json` in the app working directory.
2. Open `LocalRAG.sln` in Visual Studio.
3. Set `DemoApp` as startup project and run.

## Benchmarking at a glance

- **EE-RAG benchmark (Form1 menu):** clears/loads benchmark knowledge, runs cases, outputs summary report.
- **QA benchmark (FormQaBenchmark window):** import QA JSONL, backfill embeddings, run benchmark, save report, auto-tune model/temperature.

## Repository notes

- Primary architecture write-up: `EE-RAG-Rev4.md`.
- Unit tests: `LocalRAG.Tests/`.
- Additional provider docs: `AIClients/Docs/`.
