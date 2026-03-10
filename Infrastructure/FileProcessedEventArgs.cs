using System;

namespace ltht_project.Infrastructure
{
    internal class FileProcessedEventArgs : EventArgs
    {
        private string filePath;   // Đường dẫn của file đã được xử lý
        private bool success;   // Trạng thái thành công hay thất bại của quá trình xử lý file
        private int recordCount;   // Số lượng bản ghi đã được xử lý từ file
        private string errorMessage;   // Thông báo lỗi nếu quá trình xử lý file thất bại
        private DateTime processedTime;   // Thời gian khi file được xử lý xong

        public string FilePath { get => filePath; set => filePath = value; }
        public bool Success { get => success; set => success = value; }
        public int RecordCount { get => recordCount; set => recordCount = value; }
        public string ErrorMessage { get => errorMessage; set => errorMessage = value; }
        public DateTime ProcessedTime { get => processedTime; set => processedTime = value; }
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
