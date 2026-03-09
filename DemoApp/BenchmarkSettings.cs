using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoApp
{
    public class BenchmarkSettings
    {
        public string Provider { get; set; } = "Anthropic";
        public string Model { get; set; } = "claude-haiku-4-5-20251001";
        public int MaxQuestions { get; set; } = 50;
        public bool UseRag { get; set; } = true;
        public double Temperature { get; set; } = 0.7;

        // Thresholds — Correct and PossiblyCorrect are "at least" targets;
        // DefinitelyWrong and Indeterminate are "at most" targets.
        public double CorrectThreshold { get; set; } = 50.0;
        public double PossiblyCorrectThreshold { get; set; } = 10.0;
        public double DefinitelyWrongThreshold { get; set; } = 10.0;
        public double IndeterminateThreshold { get; set; } = 30.0;

        private static readonly JsonSerializerOptions _jsonOpts =
            new() { WriteIndented = true };

        private static string SettingsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "benchmark-settings.json");

        public static BenchmarkSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<BenchmarkSettings>(json) ?? new();
                }
            }
            catch { }
            return new();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, _jsonOpts));
            }
            catch { }
        }
    }
}
