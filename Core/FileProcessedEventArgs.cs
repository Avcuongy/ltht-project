using System;

namespace ltht_project.Core
{
    internal class FileProcessedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public bool Success { get; }
        public int RecordCount { get; }
        public string ErrorMessage { get; }
        public DateTime ProcessedTime { get; }

        public FileProcessedEventArgs(string filePath, bool success, int recordCount, string errorMessage)
        {
            FilePath = filePath;
            Success = success;
            RecordCount = recordCount;
            ErrorMessage = errorMessage;
            ProcessedTime = DateTime.Now;
        }
    }
}
