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
            txtModel = new TextBox();
            lblModel = new Label();
            cmbProvider = new ComboBox();
            lblProvider = new Label();

            rtbResults = new RichTextBox();

            grpImport.SuspendLayout();
            grpBackfill.SuspendLayout();
            grpRun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQuestions).BeginInit();
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
                lblQuestions, numQuestions, lblEmbeddedCount,
                btnRunBenchmark, btnSaveReport, chkUseRag
            });
            grpRun.Location = new Point(8, 154);
            grpRun.Name = "grpRun";
            grpRun.Size = new Size(800, 76);
            grpRun.TabIndex = 2;
            grpRun.Text = "Run Benchmark";

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
            txtModel.Size = new Size(220, 23);
            txtModel.TabIndex = 1;
            txtModel.Text = "claude-haiku-4-5-20251001";

            lblQuestions.AutoSize = true;
            lblQuestions.Location = new Point(442, 24);
            lblQuestions.Text = "Questions:";

            numQuestions.Location = new Point(510, 20);
            numQuestions.Name = "numQuestions";
            numQuestions.Minimum = 1;
            numQuestions.Maximum = 100000;
            numQuestions.Value = 50;
            numQuestions.Size = new Size(80, 23);
            numQuestions.TabIndex = 2;

            lblEmbeddedCount.AutoSize = true;
            lblEmbeddedCount.ForeColor = Color.Gray;
            lblEmbeddedCount.Location = new Point(598, 24);
            lblEmbeddedCount.Name = "lblEmbeddedCount";
            lblEmbeddedCount.Text = "(0 embedded)";

            btnRunBenchmark.Location = new Point(696, 20);
            btnRunBenchmark.Name = "btnRunBenchmark";
            btnRunBenchmark.Size = new Size(96, 25);
            btnRunBenchmark.Text = "Run";
            btnRunBenchmark.Click += btnRunBenchmark_Click;

            btnSaveReport.Enabled = false;
            btnSaveReport.Location = new Point(696, 48);
            btnSaveReport.Name = "btnSaveReport";
            btnSaveReport.Size = new Size(96, 22);
            btnSaveReport.Text = "Save Report…";
            btnSaveReport.Click += btnSaveReport_Click;

            chkUseRag.AutoSize = true;
            chkUseRag.Checked = true;
            chkUseRag.CheckState = CheckState.Checked;
            chkUseRag.Location = new Point(8, 50);
            chkUseRag.Name = "chkUseRag";
            chkUseRag.Text = "Inject similar Q&A pairs as context (requires embeddings)";
            chkUseRag.TabIndex = 3;

            // ── rtbResults ─────────────────────────────────────────────────────
            rtbResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbResults.BackColor = Color.FromArgb(30, 30, 30);
            rtbResults.ForeColor = Color.LightGray;
            rtbResults.Font = new Font("Consolas", 9F);
            rtbResults.Location = new Point(8, 238);
            rtbResults.Name = "rtbResults";
            rtbResults.ReadOnly = true;
            rtbResults.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbResults.Size = new Size(800, 410);
            rtbResults.TabIndex = 3;
            rtbResults.Text = "";

            // ── Form ───────────────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 656);
            Controls.AddRange(new Control[] { grpImport, grpBackfill, grpRun, rtbResults });
            MinimumSize = new Size(836, 694);
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
            ((System.ComponentModel.ISupportInitialize)numQuestions).EndInit();
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
        private Label lblQuestions;
        private NumericUpDown numQuestions;
        private Label lblEmbeddedCount;
        private Button btnRunBenchmark;
        private Button btnSaveReport;
        private CheckBox chkUseRag;

        private RichTextBox rtbResults;
    }
}
