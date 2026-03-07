using System;

namespace ltht_project.Infrastructure
{
    internal class FileProcessingRecord
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Checksum { get; set; }
        public DateTime ProcessedDate { get; set; }
        public ProcessingStatus Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
