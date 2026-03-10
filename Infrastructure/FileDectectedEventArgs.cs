using System;

namespace ltht_project.Infrastructure
{
    internal class FileDetectedEventArgs : EventArgs
    {
        private string filePath;   // Đường dẫn của file được phát hiện
        private DateTime detectedTime;   // Thời gian phát hiện file

        public string FilePath { get => filePath; set => filePath = value; }
        public DateTime DetectedTime { get => detectedTime; set => detectedTime = value; }
        public FileDetectedEventArgs(string filePath)
        {
            FilePath = filePath;
            DetectedTime = DateTime.Now;
        }
    }
}
