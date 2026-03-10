using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalRAG
{
    /// <summary>
    /// Configuration settings for the LocalRAG system.
    /// </summary>
    public class RAGConfiguration
    {
        private static string GetAppPath(string relativePath)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, relativePath);
        }

        /// <summary>
        /// Path to the SQLite database file for storing embeddings. Will be created if it doesn't exist.
        /// </summary>
        public string DatabasePath { get; set; } = GetAppPath(Path.Combine("Database", "Memory", "FeedbackEmbeddings512.db"));

        /// <summary>
        /// Download a MiniLM-L6-v2-ONNX model in ONNX format and save in to a folder named models along side the executable 
        /// Example: https://huggingface.co/onnx-community/all-MiniLM-L6-v2-ONNX/tree/main/onnx
        /// </summary>
        public string ModelPath { get; set; } = GetAppPath(Path.Combine("model", "model.onnx"));
        public string VocabularyPath { get; set; } = GetAppPath(Path.Combine("model", "vocab.txt"));
        public double OverlapPercentage { get; set; } = 25;
        public int NumberOfHashFunctions { get; set; } = 8;
        public int NumberOfHashTables { get; set; } = 10;
        public int MaxQueueSize { get; set; } = 1000;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
        public int InterOpNumThreads { get; set; } = 32;
        public int IntraOpNumThreads { get; set; } = 2;
 
        public int MaxCacheItems { get; set; } = 10000;
        public long CacheItemSizeThreshold { get; set; } = 1024 * 1024; // 1MB
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(15);
 
    }

}
