namespace DemoApp
{
    partial class FormQaBenchmark
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            grpImport = new GroupBox();
            lblImportStatus = new Label();
            btnImport = new Button();
            btnBrowse = new Button();
            txtFilePath = new TextBox();
            lblFile = new Label();

            grpBackfill = new GroupBox();
            btnStopBackfill = new Button();
            btnStartBackfill = new Button();
            pbarBackfill = new ProgressBar();
            lblEmbedStatus = new Label();

            grpRun = new GroupBox();
            chkUseRag = new CheckBox();
            btnSaveReport = new Button();
            btnRunBenchmark = new Button();
            lblEmbeddedCount = new Label();
            numQuestions = new NumericUpDown();
            lblQuestions = new Label();
            numTemperature = new NumericUpDown();
            lblTemperature = new Label();
            txtModel = new TextBox();
            lblModel = new Label();
            cmbProvider = new ComboBox();
            lblProvider = new Label();
            pbarRunProgress = new ProgressBar();
            lblLiveStats = new Label();

            grpThresholds = new GroupBox();
            btnSaveSettings = new Button();
            lblPctIndeterm = new Label();
            numIndetermThresh = new NumericUpDown();
            lblIndetermThresh = new Label();
            lblPctDefWrong = new Label();
            numDefWrongThresh = new NumericUpDown();
            lblDefWrongThresh = new Label();
            lblPctPossCorrect = new Label();
            numPossCorrectThresh = new NumericUpDown();
            lblPossCorrectThresh = new Label();
            lblPctCorrect = new Label();
            numCorrectThresh = new NumericUpDown();
            lblCorrectThresh = new Label();

            grpTuning = new GroupBox();
            btnStopTune = new Button();
            btnAutoTune = new Button();
            cmbTuneScore = new ComboBox();
            lblTuneScore = new Label();
            numTuneQuestions = new NumericUpDown();
            lblTuneQuestions = new Label();
            txtTuneModels = new TextBox();
            lblTuneModels = new Label();
            txtTuneTemps = new TextBox();
            lblTuneTemps = new Label();

            rtbResults = new RichTextBox();

            grpImport.SuspendLayout();
            grpBackfill.SuspendLayout();
            grpRun.SuspendLayout();
            grpThresholds.SuspendLayout();
            grpTuning.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQuestions).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTemperature).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCorrectThresh).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPossCorrectThresh).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDefWrongThresh).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIndetermThresh).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTuneQuestions).BeginInit();
            SuspendLayout();

            // ── grpImport ──────────────────────────────────────────────────────
            grpImport.Controls.AddRange(new Control[] { lblFile, txtFilePath, btnBrowse, btnImport, lblImportStatus });
            grpImport.Location = new Point(8, 8);
            grpImport.Name = "grpImport";
            grpImport.Size = new Size(800, 72);
            grpImport.TabIndex = 0;
            grpImport.Text = "Dataset Import";

            lblFile.AutoSize = true;
            lblFile.Location = new Point(8, 26);
            lblFile.Text = "File:";

            txtFilePath.Location = new Point(40, 22);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(560, 23);
            txtFilePath.TabIndex = 0;

            btnBrowse.Location = new Point(608, 21);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(80, 25);
            btnBrowse.Text = "Browse…";
            btnBrowse.Click += btnBrowse_Click;

            btnImport.Location = new Point(696, 21);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(96, 25);
            btnImport.Text = "Import";
            btnImport.Click += btnImport_Click;

            lblImportStatus.AutoSize = true;
            lblImportStatus.Location = new Point(8, 48);
            lblImportStatus.Name = "lblImportStatus";
            lblImportStatus.ForeColor = Color.Gray;
            lblImportStatus.Text = "No dataset loaded.";

            // ── grpBackfill ────────────────────────────────────────────────────
            grpBackfill.Controls.AddRange(new Control[] { lblEmbedStatus, pbarBackfill, btnStartBackfill, btnStopBackfill });
            grpBackfill.Location = new Point(8, 88);
            grpBackfill.Name = "grpBackfill";
            grpBackfill.Size = new Size(800, 58);
            grpBackfill.TabIndex = 1;
            grpBackfill.Text = "Embedding Backfill";

            lblEmbedStatus.AutoSize = true;
            lblEmbedStatus.Location = new Point(8, 24);
            lblEmbedStatus.Name = "lblEmbedStatus";
            lblEmbedStatus.Text = "Embedded: 0 / 0";

            pbarBackfill.Location = new Point(130, 22);
            pbarBackfill.Name = "pbarBackfill";
            pbarBackfill.Size = new Size(466, 20);
            pbarBackfill.TabIndex = 0;

            btnStartBackfill.Location = new Point(604, 20);
            btnStartBackfill.Name = "btnStartBackfill";
            btnStartBackfill.Size = new Size(92, 25);
            btnStartBackfill.Text = "Start Backfill";
            btnStartBackfill.Click += btnStartBackfill_Click;

            btnStopBackfill.Enabled = false;
            btnStopBackfill.Location = new Point(704, 20);
            btnStopBackfill.Name = "btnStopBackfill";
            btnStopBackfill.Size = new Size(88, 25);
            btnStopBackfill.Text = "Stop";
            btnStopBackfill.Click += btnStopBackfill_Click;

            // ── grpRun ─────────────────────────────────────────────────────────
            grpRun.Controls.AddRange(new Control[]
            {
                lblProvider, cmbProvider, lblModel, txtModel,
                lblTemperature, numTemperature,
                lblQuestions, numQuestions, lblEmbeddedCount,
                btnRunBenchmark, btnSaveReport, chkUseRag,
                pbarRunProgress, lblLiveStats
            });
            grpRun.Location = new Point(8, 154);
            grpRun.Name = "grpRun";
            grpRun.Size = new Size(800, 100);
            grpRun.TabIndex = 2;
            grpRun.Text = "Run Benchmark";

            // Row 1: Provider | Model | Temperature | Questions | Run
            lblProvider.AutoSize = true;
            lblProvider.Location = new Point(8, 24);
            lblProvider.Text = "Provider:";

            cmbProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProvider.Items.AddRange(new object[] { "Anthropic", "OpenAI", "Groq", "DeepSeek", "Grok" });
            cmbProvider.Location = new Point(62, 20);
            cmbProvider.Name = "cmbProvider";
            cmbProvider.Size = new Size(100, 23);
            cmbProvider.TabIndex = 0;
            cmbProvider.SelectedIndex = 0;

            lblModel.AutoSize = true;
            lblModel.Location = new Point(170, 24);
            lblModel.Text = "Model:";

            txtModel.Location = new Point(214, 20);
            txtModel.Name = "txtModel";
            txtModel.Size = new Size(200, 23);
            txtModel.TabIndex = 1;
            txtModel.Text = "claude-haiku-4-5-20251001";

            lblTemperature.AutoSize = true;
            lblTemperature.Location = new Point(422, 24);
            lblTemperature.Text = "Temp:";

            numTemperature.DecimalPlaces = 2;
            numTemperature.Increment = 0.05M;
            numTemperature.Minimum = 0M;
            numTemperature.Maximum = 2M;
            numTemperature.Value = 0.7M;
            numTemperature.Location = new Point(462, 20);
            numTemperature.Name = "numTemperature";
            numTemperature.Size = new Size(58, 23);
            numTemperature.TabIndex = 2;

            lblQuestions.AutoSize = true;
            lblQuestions.Location = new Point(528, 24);
            lblQuestions.Text = "Questions:";

            numQuestions.Location = new Point(596, 20);
            numQuestions.Name = "numQuestions";
            numQuestions.Minimum = 1;
            numQuestions.Maximum = 100000;
            numQuestions.Value = 50;
            numQuestions.Size = new Size(72, 23);
            numQuestions.TabIndex = 3;

            btnRunBenchmark.Location = new Point(700, 20);
            btnRunBenchmark.Name = "btnRunBenchmark";
            btnRunBenchmark.Size = new Size(92, 25);
            btnRunBenchmark.Text = "Run";
            btnRunBenchmark.Click += btnRunBenchmark_Click;

            // Row 2: UseRag | EmbeddedCount | Save Report
            chkUseRag.AutoSize = true;
            chkUseRag.Checked = true;
            chkUseRag.CheckState = CheckState.Checked;
            chkUseRag.Location = new Point(8, 50);
            chkUseRag.Name = "chkUseRag";
            chkUseRag.Text = "Inject similar Q&A pairs as context (requires embeddings)";
            chkUseRag.TabIndex = 4;

            lblEmbeddedCount.AutoSize = true;
            lblEmbeddedCount.ForeColor = Color.Gray;
            lblEmbeddedCount.Location = new Point(462, 52);
            lblEmbeddedCount.Name = "lblEmbeddedCount";
            lblEmbeddedCount.Text = "(0 embedded)";

            btnSaveReport.Enabled = false;
            btnSaveReport.Location = new Point(700, 48);
            btnSaveReport.Name = "btnSaveReport";
            btnSaveReport.Size = new Size(92, 22);
            btnSaveReport.Text = "Save Report…";
            btnSaveReport.Click += btnSaveReport_Click;

            // Row 3: Live progress bar + running accuracy stats
            pbarRunProgress.Location = new Point(8, 76);
            pbarRunProgress.Name = "pbarRunProgress";
            pbarRunProgress.Size = new Size(470, 16);
            pbarRunProgress.TabIndex = 5;

            lblLiveStats.AutoSize = true;
            lblLiveStats.Font = new Font("Consolas", 8.25F);
            lblLiveStats.ForeColor = Color.DimGray;
            lblLiveStats.Location = new Point(486, 77);
            lblLiveStats.Name = "lblLiveStats";
            lblLiveStats.Text = "Q: 0/0  ✓--%  ?--%  ✗--%  ---%";

            // ── grpThresholds ──────────────────────────────────────────────────
            grpThresholds.Controls.AddRange(new Control[]
            {
                lblCorrectThresh, numCorrectThresh, lblPctCorrect,
                lblPossCorrectThresh, numPossCorrectThresh, lblPctPossCorrect,
                lblDefWrongThresh, numDefWrongThresh, lblPctDefWrong,
                lblIndetermThresh, numIndetermThresh, lblPctIndeterm,
                btnSaveSettings
            });
            grpThresholds.Location = new Point(8, 262);
            grpThresholds.Name = "grpThresholds";
            grpThresholds.Size = new Size(800, 54);
            grpThresholds.TabIndex = 3;
            grpThresholds.Text = "Score Thresholds";

            lblCorrectThresh.AutoSize = true;
            lblCorrectThresh.Location = new Point(8, 24);
            lblCorrectThresh.Text = "Correct ≥";

            numCorrectThresh.DecimalPlaces = 1;
            numCorrectThresh.Increment = 5M;
            numCorrectThresh.Minimum = 0M;
            numCorrectThresh.Maximum = 100M;
            numCorrectThresh.Value = 50M;
            numCorrectThresh.Location = new Point(74, 20);
            numCorrectThresh.Name = "numCorrectThresh";
            numCorrectThresh.Size = new Size(52, 23);
            numCorrectThresh.TabIndex = 0;

            lblPctCorrect.AutoSize = true;
            lblPctCorrect.Location = new Point(128, 24);
            lblPctCorrect.Text = "%";

            lblPossCorrectThresh.AutoSize = true;
            lblPossCorrectThresh.Location = new Point(148, 24);
            lblPossCorrectThresh.Text = "Poss. Correct ≥";

            numPossCorrectThresh.DecimalPlaces = 1;
            numPossCorrectThresh.Increment = 5M;
            numPossCorrectThresh.Minimum = 0M;
            numPossCorrectThresh.Maximum = 100M;
            numPossCorrectThresh.Value = 10M;
            numPossCorrectThresh.Location = new Point(248, 20);
            numPossCorrectThresh.Name = "numPossCorrectThresh";
            numPossCorrectThresh.Size = new Size(52, 23);
            numPossCorrectThresh.TabIndex = 1;

            lblPctPossCorrect.AutoSize = true;
            lblPctPossCorrect.Location = new Point(302, 24);
            lblPctPossCorrect.Text = "%";

            lblDefWrongThresh.AutoSize = true;
            lblDefWrongThresh.Location = new Point(322, 24);
            lblDefWrongThresh.Text = "Def. Wrong ≤";

            numDefWrongThresh.DecimalPlaces = 1;
            numDefWrongThresh.Increment = 5M;
            numDefWrongThresh.Minimum = 0M;
            numDefWrongThresh.Maximum = 100M;
            numDefWrongThresh.Value = 10M;
            numDefWrongThresh.Location = new Point(408, 20);
            numDefWrongThresh.Name = "numDefWrongThresh";
            numDefWrongThresh.Size = new Size(52, 23);
            numDefWrongThresh.TabIndex = 2;

            lblPctDefWrong.AutoSize = true;
            lblPctDefWrong.Location = new Point(462, 24);
            lblPctDefWrong.Text = "%";

            lblIndetermThresh.AutoSize = true;
            lblIndetermThresh.Location = new Point(482, 24);
            lblIndetermThresh.Text = "Indeterm. ≤";

            numIndetermThresh.DecimalPlaces = 1;
            numIndetermThresh.Increment = 5M;
            numIndetermThresh.Minimum = 0M;
            numIndetermThresh.Maximum = 100M;
            numIndetermThresh.Value = 30M;
            numIndetermThresh.Location = new Point(562, 20);
            numIndetermThresh.Name = "numIndetermThresh";
            numIndetermThresh.Size = new Size(52, 23);
            numIndetermThresh.TabIndex = 3;

            lblPctIndeterm.AutoSize = true;
            lblPctIndeterm.Location = new Point(616, 24);
            lblPctIndeterm.Text = "%";

            btnSaveSettings.Location = new Point(700, 20);
            btnSaveSettings.Name = "btnSaveSettings";
            btnSaveSettings.Size = new Size(92, 25);
            btnSaveSettings.Text = "Save Settings";
            btnSaveSettings.Click += btnSaveSettings_Click;

            // ── grpTuning ──────────────────────────────────────────────────────
            grpTuning.Controls.AddRange(new Control[]
            {
                lblTuneTemps, txtTuneTemps,
                lblTuneModels, txtTuneModels,
                lblTuneQuestions, numTuneQuestions,
                lblTuneScore, cmbTuneScore,
                btnAutoTune, btnStopTune
            });
            grpTuning.Location = new Point(8, 324);
            grpTuning.Name = "grpTuning";
            grpTuning.Size = new Size(800, 76);
            grpTuning.TabIndex = 4;
            grpTuning.Text = "Auto-Tune";

            // Row 1: Temperatures | Models
            lblTuneTemps.AutoSize = true;
            lblTuneTemps.Location = new Point(8, 24);
            lblTuneTemps.Text = "Temperatures:";

            txtTuneTemps.Location = new Point(92, 20);
            txtTuneTemps.Name = "txtTuneTemps";
            txtTuneTemps.Size = new Size(210, 23);
            txtTuneTemps.Text = "0.0, 0.3, 0.5, 0.7, 1.0, 1.5";
            txtTuneTemps.TabIndex = 0;

            lblTuneModels.AutoSize = true;
            lblTuneModels.Location = new Point(312, 24);
            lblTuneModels.Text = "Models (csv):";

            txtTuneModels.Location = new Point(398, 20);
            txtTuneModels.Name = "txtTuneModels";
            txtTuneModels.Size = new Size(294, 23);
            txtTuneModels.TabIndex = 1;
            txtTuneModels.PlaceholderText = "leave blank to use current model";

            // Row 2: Questions | Score | Auto-Tune | Stop
            lblTuneQuestions.AutoSize = true;
            lblTuneQuestions.Location = new Point(8, 52);
            lblTuneQuestions.Text = "Qs/run:";

            numTuneQuestions.Location = new Point(58, 48);
            numTuneQuestions.Name = "numTuneQuestions";
            numTuneQuestions.Minimum = 5;
            numTuneQuestions.Maximum = 500;
            numTuneQuestions.Value = 10;
            numTuneQuestions.Size = new Size(58, 23);
            numTuneQuestions.TabIndex = 2;

            lblTuneScore.AutoSize = true;
            lblTuneScore.Location = new Point(126, 52);
            lblTuneScore.Text = "Score by:";

            cmbTuneScore.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTuneScore.Items.AddRange(new object[]
            {
                "Correct %",
                "Correct + Possibly Correct %",
                "Composite (Correct + 0.5×Poss − 2×Wrong)"
            });
            cmbTuneScore.Location = new Point(186, 48);
            cmbTuneScore.Name = "cmbTuneScore";
            cmbTuneScore.Size = new Size(280, 23);
            cmbTuneScore.TabIndex = 3;
            cmbTuneScore.SelectedIndex = 0;

            btnAutoTune.Location = new Point(590, 48);
            btnAutoTune.Name = "btnAutoTune";
            btnAutoTune.Size = new Size(100, 25);
            btnAutoTune.Text = "Auto-Tune";
            btnAutoTune.Click += btnAutoTune_Click;

            btnStopTune.Enabled = false;
            btnStopTune.Location = new Point(700, 48);
            btnStopTune.Name = "btnStopTune";
            btnStopTune.Size = new Size(92, 25);
            btnStopTune.Text = "Stop";
            btnStopTune.Click += btnStopTune_Click;

            // ── rtbResults ─────────────────────────────────────────────────────
            rtbResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbResults.BackColor = Color.FromArgb(30, 30, 30);
            rtbResults.ForeColor = Color.LightGray;
            rtbResults.Font = new Font("Consolas", 9F);
            rtbResults.Location = new Point(8, 408);
            rtbResults.Name = "rtbResults";
            rtbResults.ReadOnly = true;
            rtbResults.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbResults.Size = new Size(800, 340);
            rtbResults.TabIndex = 5;
            rtbResults.Text = "";

            // ── Form ───────────────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 756);
            Controls.AddRange(new Control[] { grpImport, grpBackfill, grpRun, grpThresholds, grpTuning, rtbResults });
            MinimumSize = new Size(836, 794);
            Name = "FormQaBenchmark";
            Text = "QA Dataset Benchmark";
            Load += FormQaBenchmark_Load;
            FormClosing += FormQaBenchmark_FormClosing;

            grpImport.ResumeLayout(false);
            grpImport.PerformLayout();
            grpBackfill.ResumeLayout(false);
            grpBackfill.PerformLayout();
            grpRun.ResumeLayout(false);
            grpRun.PerformLayout();
            grpThresholds.ResumeLayout(false);
            grpThresholds.PerformLayout();
            grpTuning.ResumeLayout(false);
            grpTuning.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numQuestions).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTemperature).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCorrectThresh).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPossCorrectThresh).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDefWrongThresh).EndInit();
            ((System.ComponentModel.ISupportInitialize)numIndetermThresh).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTuneQuestions).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpImport;
        private Label lblFile;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnImport;
        private Label lblImportStatus;

        private GroupBox grpBackfill;
        private Label lblEmbedStatus;
        private ProgressBar pbarBackfill;
        private Button btnStartBackfill;
        private Button btnStopBackfill;

        private GroupBox grpRun;
        private Label lblProvider;
        private ComboBox cmbProvider;
        private Label lblModel;
        private TextBox txtModel;
        private Label lblTemperature;
        private NumericUpDown numTemperature;
        private Label lblQuestions;
        private NumericUpDown numQuestions;
        private Label lblEmbeddedCount;
        private Button btnRunBenchmark;
        private Button btnSaveReport;
        private CheckBox chkUseRag;
        private ProgressBar pbarRunProgress;
        private Label lblLiveStats;

        private GroupBox grpThresholds;
        private Label lblCorrectThresh;
        private NumericUpDown numCorrectThresh;
        private Label lblPctCorrect;
        private Label lblPossCorrectThresh;
        private NumericUpDown numPossCorrectThresh;
        private Label lblPctPossCorrect;
        private Label lblDefWrongThresh;
        private NumericUpDown numDefWrongThresh;
        private Label lblPctDefWrong;
        private Label lblIndetermThresh;
        private NumericUpDown numIndetermThresh;
        private Label lblPctIndeterm;
        private Button btnSaveSettings;

        private GroupBox grpTuning;
        private Label lblTuneTemps;
        private TextBox txtTuneTemps;
        private Label lblTuneModels;
        private TextBox txtTuneModels;
        private Label lblTuneQuestions;
        private NumericUpDown numTuneQuestions;
        private Label lblTuneScore;
        private ComboBox cmbTuneScore;
        private Button btnAutoTune;
        private Button btnStopTune;

        private RichTextBox rtbResults;
    }
}
