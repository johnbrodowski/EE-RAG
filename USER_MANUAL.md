# EE-RAG User Manual

This manual is for users running the `DemoApp` WinForms interface.

## 1) Before you start

## 1.1 Required setup

- Build/run on a Windows environment that supports `net10.0-windows7.0` WinForms.
- Ensure AI provider settings are available through `ai-settings.json` in the app working directory.
- Ensure embedding model files are available if you plan to generate embeddings/backfill.

## 1.2 Data stores used by the app

- Main conversational embedding store (configured through `RAGConfiguration` and used by `Form1`).
- QA benchmark store (separate DB):
  - `Database/QaDataset/QaBenchmark.db` under the app base directory.

---

## 2) Main window (`Form1`) usage

## 2.1 Ask AI (EE-RAG pipeline)

1. Enter a question in the query box.
2. Select **Provider** and **Model**.
3. Optional: toggle **Silent Mode**.
4. Click **Ask AI**.

Expected behavior:

- The app runs the EE-RAG pipeline and prints a detailed trace:
  - candidate list surfaced,
  - first response,
  - whether retrieval occurred,
  - final response,
  - two-phase audit fields.

If provider/model is missing, the app returns a validation error immediately.

## 2.2 Search (RAG feedback preview)

1. Enter a query.
2. Click **Search**.

The app returns a ranked set of similar prior feedback records with similarity scores and truncated content sections.

## 2.3 Run tests (menu action)

- Use **Run Tests** menu item.
- The app executes test routines and prints output in the results pane.
- Integration-style checks are attempted if model files are available; otherwise they are skipped.

## 2.4 EE-RAG benchmark (menu action)

1. Choose provider/model.
2. Start benchmark from menu.
3. Confirm database reset prompt (this clears existing records when enabled).

The benchmark seeds benchmark knowledge, runs benchmark cases, and prints a summary report.

## 2.5 Generate mock data

- Use the menu action for mock data generation.
- The app creates sample records and displays before/after stats.

## 2.6 Embedding backfill (main DB)

- **Missing**: backfills only missing embeddings.
- **All**: regenerates/backfills broadly (implementation-defined by DB method).

Progress is shown live. Errors are printed in the result pane.

---

## 3) QA Dataset Benchmark window (`FormQaBenchmark`)

Open from main menu (QA dataset benchmark option).

## 3.1 Controls overview

- **Settings section**
  - Provider, model, max questions, temperature, use RAG toggle.
- **Threshold section**
  - Correct / Possibly Correct / Definitely Wrong / Indeterminate thresholds.
- **Import section**
  - File picker + import status.
- **Backfill section**
  - Start/stop embedding backfill and embedded count progress.
- **Run section**
  - Run benchmark and save report.
- **Auto-Tune section**
  - Temperature list, model list, questions per run, scoring metric, start/stop tune.

## 3.2 Importing QA data

1. Click **Browse** and select a `.jsonl` file.
2. Click **Import**.
3. Wait for progress/status updates.

Accepted records are inserted into the QA benchmark DB. Invalid file path or DB state errors are shown in red status text.

## 3.3 Backfilling question embeddings

1. Click **Start Backfill**.
2. Monitor progress bar and embedded counters.
3. Click **Stop Backfill** to stop cooperatively after current item.

This prepares dataset items for RAG-context-assisted runs.

## 3.4 Running a QA benchmark

1. Set provider/model and run parameters.
2. Optionally adjust thresholds.
3. Click **Run Benchmark**.

During run:

- Per-question progress lines appear in the log.
- Live stats show counts and percentages.

After run:

- Summary with pass/fail threshold markers appears in results pane.
- **Save Report** becomes enabled.

## 3.5 Saving benchmark report

1. Click **Save Report**.
2. Choose output path.
3. Report is written as plain text.

## 3.6 Auto-Tune model and temperature

1. Enter comma-separated temperatures (e.g., `0.0,0.3,0.7`).
2. Optional comma-separated models (blank = current model only).
3. Set questions per run and score metric.
4. Click **Auto-Tune**.

Behavior:

- Runs every model/temperature combination.
- Logs each combo's outcome rates and computed score.
- Applies the best model/temperature to the main run controls.

Use **Stop Tune** to cancel after current run.

---

## 4) Understanding QA outcomes

Each question is classified as one of:

- **Correct**
- **Possibly Correct**
- **Definitely Wrong**
- **Indeterminate**

Classification is based on matching model output against dataset prediction lists (case-insensitive substring matching).

---

## 5) Troubleshooting

## 5.1 “Provider and Model are required”

Set both provider and model fields before running Ask AI or benchmark operations.

## 5.2 “Cannot load BERT model” / embedding issues

Embedding/backfill requires valid embedding model configuration and files. Verify local model path/config.

## 5.3 `ai-settings.json` errors

If missing or malformed, provider initialization will fail. Start from `AIClients/Docs/Examples/ai-settings.example.json`.

## 5.4 Benchmark appears weak or noisy

- Increase question count.
- Tune temperature downward for deterministic outputs.
- Use Auto-Tune with a composite score.
- Ensure embeddings were backfilled before enabling RAG context.

---

## 6) Operational cautions

- EE-RAG benchmark flow can clear existing DB records when seeding fresh data.
- Backfill and benchmark jobs are long-running; avoid closing app mid-run when possible.
- QA benchmark DB is distinct from main EE-RAG DB, but both are local and stateful.
