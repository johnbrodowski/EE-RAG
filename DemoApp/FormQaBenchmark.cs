using AiMessagingCore.Configuration;
using AiMessagingCore.Core;

using LocalRAG;
using LocalRAG.QaDataset;

namespace DemoApp
{
    public partial class FormQaBenchmark : Form
    {
        private QaDatasetDatabase? _db;
        private readonly QaEmbeddingBackfiller _backfiller = new();
        private QaBenchmarkReport? _lastReport;
        private BenchmarkSettings _settings = new();

        private static string DefaultDbPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                         "Database", "QaDataset", "QaBenchmark.db");

        public FormQaBenchmark()
        {
            InitializeComponent();
        }

        private async void FormQaBenchmark_Load(object sender, EventArgs e)
        {
            LoadSettings();
            await OpenDatabaseAsync(DefaultDbPath);
        }

        // ── Settings ──────────────────────────────────────────────────────────

        private void LoadSettings()
        {
            _settings = BenchmarkSettings.Load();

            // Restore run controls
            var providerIdx = cmbProvider.Items.IndexOf(_settings.Provider);
            cmbProvider.SelectedIndex = providerIdx >= 0 ? providerIdx : 0;
            txtModel.Text = _settings.Model;
            numQuestions.Value = Math.Max(numQuestions.Minimum,
                                 Math.Min(numQuestions.Maximum, _settings.MaxQuestions));
            numTemperature.Value = (decimal)Math.Max(0, Math.Min(2, _settings.Temperature));
            chkUseRag.Checked = _settings.UseRag;

            // Restore thresholds
            numCorrectThresh.Value = (decimal)Math.Max(0, Math.Min(100, _settings.CorrectThreshold));
            numPossCorrectThresh.Value = (decimal)Math.Max(0, Math.Min(100, _settings.PossiblyCorrectThreshold));
            numDefWrongThresh.Value = (decimal)Math.Max(0, Math.Min(100, _settings.DefinitelyWrongThreshold));
            numIndetermThresh.Value = (decimal)Math.Max(0, Math.Min(100, _settings.IndeterminateThreshold));
        }

        private void SaveCurrentSettings()
        {
            _settings.Provider = cmbProvider.Text.Trim();
            _settings.Model = txtModel.Text.Trim();
            _settings.MaxQuestions = (int)numQuestions.Value;
            _settings.Temperature = (double)numTemperature.Value;
            _settings.UseRag = chkUseRag.Checked;
            _settings.CorrectThreshold = (double)numCorrectThresh.Value;
            _settings.PossiblyCorrectThreshold = (double)numPossCorrectThresh.Value;
            _settings.DefinitelyWrongThreshold = (double)numDefWrongThresh.Value;
            _settings.IndeterminateThreshold = (double)numIndetermThresh.Value;
            _settings.Save();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            SaveCurrentSettings();
            AppendLine("Settings saved.");
        }

        // ── Database ──────────────────────────────────────────────────────────

        private async Task OpenDatabaseAsync(string path)
        {
            _db?.Dispose();
            _db = new QaDatasetDatabase(path);
            await _db.InitializeAsync();
            await RefreshCountsAsync();
        }

        private async Task RefreshCountsAsync()
        {
            if (_db == null) return;
            var (total, embedded) = await _db.GetItemCountAsync();
            lblEmbedStatus.Text = $"Embedded: {embedded:N0} / {total:N0}";
            lblEmbeddedCount.Text = $"({embedded:N0} embedded)";

            if (total > 0)
            {
                pbarBackfill.Maximum = total;
                pbarBackfill.Value = Math.Min(embedded, total);
            }
            else
            {
                pbarBackfill.Value = 0;
            }
        }

        // ── Import ────────────────────────────────────────────────────────────

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select JSONL Dataset File",
                Filter = "JSONL files (*.jsonl)|*.jsonl|JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtFilePath.Text = dlg.FileName;
        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
            var path = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                lblImportStatus.Text = "File not found.";
                lblImportStatus.ForeColor = Color.Red;
                return;
            }

            if (_db == null)
            {
                lblImportStatus.Text = "Database not ready.";
                lblImportStatus.ForeColor = Color.Red;
                return;
            }

            btnImport.Enabled = false;
            lblImportStatus.ForeColor = Color.DarkBlue;
            lblImportStatus.Text = "Importing…";

            try
            {
                var progress = new Progress<string>(msg =>
                {
                    lblImportStatus.Text = msg;
                    lblImportStatus.ForeColor = Color.DarkBlue;
                });

                int count = await QaDatasetLoader.ImportFileAsync(path, _db, progress);
                lblImportStatus.Text = $"Imported {count:N0} items from {Path.GetFileName(path)}.";
                lblImportStatus.ForeColor = Color.DarkGreen;
                await RefreshCountsAsync();
            }
            catch (Exception ex)
            {
                lblImportStatus.Text = $"Error: {ex.Message}";
                lblImportStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnImport.Enabled = true;
            }
        }

        // ── Backfill ──────────────────────────────────────────────────────────

        private void btnStartBackfill_Click(object sender, EventArgs e)
        {
            if (_db == null) return;
            if (_backfiller.IsRunning) return;

            EmbedderClassNew embedder;
            try
            {
                embedder = new EmbedderClassNew(new RAGConfiguration());
            }
            catch (Exception ex)
            {
                AppendLine($"[Backfill] Cannot load BERT model: {ex.Message}");
                return;
            }

            btnStartBackfill.Enabled = false;
            btnStopBackfill.Enabled = true;
            AppendLine("[Backfill] Starting…");

            var progress = new Progress<(int Done, int Total)>(p =>
            {
                if (p.Total > 0)
                {
                    pbarBackfill.Maximum = p.Total;
                    pbarBackfill.Value = Math.Min(p.Done, p.Total);
                    lblEmbedStatus.Text = $"Embedded: {p.Done:N0} / {p.Total:N0}";
                    lblEmbeddedCount.Text = $"({p.Done:N0} embedded)";
                }
            });

            _backfiller.Start(_db, embedder, progress, errMsg =>
            {
                Invoke(() => AppendLine($"[Backfill] Error: {errMsg}"));
                Invoke(() => ResetBackfillButtons());
            });

            // Poll for completion
            _ = Task.Run(async () =>
            {
                await _backfiller.WaitAsync();
                Invoke(() =>
                {
                    AppendLine("[Backfill] Complete.");
                    ResetBackfillButtons();
                    _ = RefreshCountsAsync();
                });
            });
        }

        private void btnStopBackfill_Click(object sender, EventArgs e)
        {
            _backfiller.Stop();
            AppendLine("[Backfill] Stopping after current item…");
            btnStopBackfill.Enabled = false;
        }

        private void ResetBackfillButtons()
        {
            btnStartBackfill.Enabled = true;
            btnStopBackfill.Enabled = false;
        }

        // ── Benchmark run ─────────────────────────────────────────────────────

        private async void btnRunBenchmark_Click(object sender, EventArgs e)
        {
            if (_db == null) return;

            var provider = cmbProvider.Text.Trim();
            var model = txtModel.Text.Trim();
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(model))
            {
                AppendLine("Error: Provider and Model are required.");
                return;
            }

            // Save settings on every run so they persist
            SaveCurrentSettings();

            btnRunBenchmark.Enabled = false;
            btnSaveReport.Enabled = false;
            _lastReport = null;

            try
            {
                var config = AiSettings.LoadFromFile("ai-settings.json");
                AiSettings.ApplyToEnvironment(config);

                var temperature = (double)numTemperature.Value;

                var session = AiSessionBuilder
                    .WithProvider(provider)
                    .WithModel(model)
                    .WithTemperature(temperature)
                    .WithMaxTokens(256)
                    .WithSystemMessage(
                        "You are a factual Q&A assistant. Answer questions concisely and directly. " +
                        "Give only the answer — do not explain or add qualifications unless they are part of the answer.")
                    .Build();

                var opts = new QaBenchmarkOptions
                {
                    MaxQuestions = (int)numQuestions.Value,
                    Provider = provider,
                    Model = model,
                    Temperature = temperature,
                    UseRagContext = chkUseRag.Checked,
                    SimilarContextCount = 3
                };

                var progress = new Progress<string>(msg => AppendLine(msg));

                AppendLine($"=== Starting benchmark: {opts.MaxQuestions} questions, provider={provider}, model={model}, temp={temperature:F2} ===");

                _lastReport = await QaBenchmarkRunner.RunAsync(opts, _db, session, progress);

                // Populate report metadata
                _lastReport.Provider = provider;
                _lastReport.Model = model;
                _lastReport.Temperature = temperature;
                _lastReport.CorrectThreshold = (double)numCorrectThresh.Value;
                _lastReport.PossiblyCorrectThreshold = (double)numPossCorrectThresh.Value;
                _lastReport.DefinitelyWrongThreshold = (double)numDefWrongThresh.Value;
                _lastReport.IndeterminateThreshold = (double)numIndetermThresh.Value;

                rtbResults.AppendText(Environment.NewLine + _lastReport.FormatSummary());
                rtbResults.ScrollToCaret();
                btnSaveReport.Enabled = true;
            }
            catch (Exception ex)
            {
                AppendLine($"Error: {ex.Message}");
            }
            finally
            {
                btnRunBenchmark.Enabled = true;
            }
        }

        // ── Save report ───────────────────────────────────────────────────────

        private void btnSaveReport_Click(object sender, EventArgs e)
        {
            if (_lastReport == null) return;

            using var dlg = new SaveFileDialog
            {
                Title = "Save Benchmark Report",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"qa_benchmark_{_lastReport.RunId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(dlg.FileName, _lastReport.FormatSummary());
                    AppendLine($"Report saved to: {dlg.FileName}");
                }
                catch (Exception ex)
                {
                    AppendLine($"Save failed: {ex.Message}");
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void AppendLine(string text)
        {
            if (InvokeRequired) { Invoke(() => AppendLine(text)); return; }
            rtbResults.AppendText(text + Environment.NewLine);
            rtbResults.ScrollToCaret();
        }

        private async void FormQaBenchmark_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backfiller.Stop();
            await _backfiller.WaitAsync();
            _db?.Dispose();
        }
    }
}
