using System;

namespace ltht_project.Infrastructure
{
    internal class FileProcessingRecord
    {
        private string fileName;   // Tên file
        private string filePath;   // Đường dẫn file
        private string checksum;   // Giá trị checksum của file để xác định nếu file đã được xử lý trước đó
        private DateTime processedDate;   // Ngày giờ khi file được xử lý
        private ProcessingStatus status;   // Trạng thái xử lý của file (Processing, Success, Failed)
        private DateTime processedTime;   // Thời gian cụ thể khi file được xử lý hoàn thành
        private string errorMessage;   // Lỗi nếu có trong quá trình xử lý file

        public string FileName { get => fileName; set => fileName = value; }
        public string FilePath { get => filePath; set => filePath = value; }
        public string Checksum { get => checksum; set => checksum = value; }
        public DateTime ProcessedDate { get => processedDate; set => processedDate = value; }
        public DateTime ProcessedTime { get => processedTime; set => processedTime = value; }
        public string ErrorMessage { get => errorMessage; set => errorMessage = value; }
        public ProcessingStatus Status { get => status; set => status = value; }
    }
}
