using System.Text;

namespace LocalRAG.QaDataset
{
    public class QaBenchmarkReport
    {
        public string RunId { get; set; } = "";
        public int TotalRun { get; set; }
        public int Correct { get; set; }
        public int PossiblyCorrect { get; set; }
        public int DefinitelyWrong { get; set; }
        public int Indeterminate { get; set; }
        public TimeSpan Duration { get; set; }
        public List<QaRunResult> Results { get; set; } = new();

        public double CorrectRate => TotalRun == 0 ? 0 : (double)Correct / TotalRun * 100;
        public double PossiblyCorrectRate => TotalRun == 0 ? 0 : (double)PossiblyCorrect / TotalRun * 100;
        public double DefinitelyWrongRate => TotalRun == 0 ? 0 : (double)DefinitelyWrong / TotalRun * 100;
        public double IndeterminateRate => TotalRun == 0 ? 0 : (double)Indeterminate / TotalRun * 100;

        public string FormatSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== QA Benchmark Report (RunId: {RunId}) ===");
            sb.AppendLine($"Generated : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Duration  : {Duration:mm\\:ss\\.fff}");
            sb.AppendLine();
            sb.AppendLine($"Questions tested  : {TotalRun}");
            sb.AppendLine($"Correct           : {Correct,5} ({CorrectRate:F1}%)");
            sb.AppendLine($"Possibly Correct  : {PossiblyCorrect,5} ({PossiblyCorrectRate:F1}%)");
            sb.AppendLine($"Definitely Wrong  : {DefinitelyWrong,5} ({DefinitelyWrongRate:F1}%)");
            sb.AppendLine($"Indeterminate     : {Indeterminate,5} ({IndeterminateRate:F1}%)");
            sb.AppendLine();
            sb.AppendLine("─── Per-Question Results ───────────────────────────────────────────");

            foreach (var r in Results)
            {
                var icon = r.Outcome switch
                {
                    QaOutcome.Correct          => "[+]",
                    QaOutcome.PossiblyCorrect  => "[?]",
                    QaOutcome.DefinitelyWrong  => "[X]",
                    _                          => "[-]"
                };
                var q = r.Question.Length > 70 ? r.Question[..70] + "…" : r.Question;
                sb.AppendLine($"{icon} {q}");

                if (!string.IsNullOrEmpty(r.MatchedAnswer))
                    sb.AppendLine($"    Matched : \"{r.MatchedAnswer}\"");

                var resp = r.ModelResponse ?? "(no response)";
                if (resp.Length > 140) resp = resp[..140] + "…";
                sb.AppendLine($"    Response: {resp}");
            }

            return sb.ToString();
        }
    }
}
