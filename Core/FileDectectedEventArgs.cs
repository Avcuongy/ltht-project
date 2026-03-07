using System;

namespace ltht_project.Core
{
    internal class FileDetectedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public DateTime DetectedTime { get; }

        public FileDetectedEventArgs(string filePath)
        {
            FilePath = filePath;
            DetectedTime = DateTime.Now;
        }
    }
}
