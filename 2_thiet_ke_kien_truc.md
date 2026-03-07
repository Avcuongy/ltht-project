## 2.3. Thiết kế kiến trúc và luồng xử lý

### 2.3.1. Kiến trúc nhiều lớp
Hệ thống được tổ chức theo các lớp chức năng riêng biệt nhằm tăng khả năng mở rộng và bảo trì:
- Lớp `Presentation` (UI / API): Tiếp nhận yêu cầu, cấu hình và hiển thị trạng thái xử lý.
- Lớp `Application` (Service): Điều phối luồng xử lý, gọi tới các thành phần Producer/Consumer, quản lý workflow.
- Lớp `Domain`: Chứa các nghiệp vụ lõi, quy tắc xử lý tệp, mô hình dữ liệu và logic kiểm tra ràng buộc.
- Lớp `Infrastructure`: Làm việc với hệ thống file, hàng đợi, cơ sở dữ liệu, logging, cấu hình, dịch vụ ngoài.

Cấu trúc theo lớp giúp tách biệt rõ ràng giữa phần hiển thị, điều phối, nghiệp vụ và hạ tầng, cho phép thay đổi từng thành phần mà không ảnh hưởng toàn hệ thống.

### 2.3.2. Mô hình Producer-Consumer
Hệ thống áp dụng mô hình Producer-Consumer để xử lý song song nhiều tệp tin:
- `Producer` chịu trách nhiệm giám sát thư mục/nguồn dữ liệu, phát hiện tệp tin mới và đưa thông tin tệp vào hàng đợi.
- `Consumer` đọc từ hàng đợi, thực hiện kiểm tra trùng lặp, xử lý nội dung, lưu kết quả và cập nhật trạng thái.
- Hàng đợi trung gian (in-memory hoặc message queue) đảm bảo tách rời giữa khâu phát hiện tệp và khâu xử lý, giúp dễ dàng scale-out số lượng Consumer.

Cách tiếp cận này giúp tận dụng tài nguyên đa luồng, giới hạn số tác vụ xử lý đồng thời, tránh quá tải tài nguyên hệ thống.

### 2.3.3. Quy trình tự động nhận diện và xử lý tệp tin mới theo thời gian thực
1. `File Watcher` (thuộc Producer) lắng nghe thay đổi trên thư mục nguồn.
2. Khi xuất hiện tệp tin mới, hệ thống kiểm tra điều kiện ban đầu (định dạng, kích thước, trạng thái sẵn sàng).
3. Thông tin siêu dữ liệu tệp (đường dẫn, thời gian, checksum tạm thời, loại tệp) được đẩy vào hàng đợi.
4. Một `Consumer` nhận nhiệm vụ từ hàng đợi, thực hiện:
   - Kiểm tra trùng lặp.
   - Đọc và phân tích nội dung.
   - Tính toán/biến đổi dữ liệu theo nghiệp vụ.
   - Ghi kết quả ra kho lưu trữ (DB, data lake, index...).
5. Trạng thái xử lý, log chi tiết và các lỗi phát sinh được lưu lại để phục vụ giám sát, audit và retry.
6. Hệ thống cung cấp API/UI để theo dõi tiến độ xử lý theo thời gian thực.

## 2.4. Chiến lược tối ưu

### 2.4.1. Thuật toán cập nhật cộng dồn (Incremental)
- Lưu lại trạng thái xử lý lần gần nhất (checkpoint), ví dụ: thời gian xử lý cuối, danh sách các tệp đã hoàn thành, phiên bản dữ liệu.
- Khi có dữ liệu mới, hệ thống chỉ tải và xử lý phần gia tăng (tệp mới hoặc phần nội dung mới), tránh việc tính toán lại toàn bộ.
- Ứng dụng cơ chế `delta`/`diff` để so sánh, chỉ cập nhật các bản ghi thay đổi.
- Giảm thời gian xử lý, tiết kiệm tài nguyên I/O và CPU, phù hợp với yêu cầu chạy liên tục.

### 2.4.2. Cơ chế chống trùng lặp
- Gán mỗi tệp một `FileId` duy nhất dựa trên tổ hợp: đường dẫn tương đối + thời gian tạo + hash nội dung.
- Lưu `FileId` và trạng thái xử lý vào cơ sở dữ liệu hoặc kho meta-data.
- Trước khi xử lý, Consumer kiểm tra:
  - Nếu `FileId` đã tồn tại và ở trạng thái `Completed` → bỏ qua.
  - Nếu ở trạng thái `Processing`/`Failed` → áp dụng chính sách retry hoặc skip theo cấu hình.
- Có thể bổ sung cơ chế `lock`/`lease` để đảm bảo cùng một tệp không bị nhiều Consumer xử lý đồng thời.

### 2.4.3. Thiết kế chạy liên tục và tự động phục hồi
- Dịch vụ xử lý chạy dạng Windows Service/Background Service, luôn hoạt động ở chế độ daemon.
- Sử dụng cơ chế `retry` có backoff cho các lỗi tạm thời (mất kết nối DB, file đang bị khóa...).
- Ghi log chi tiết và metric (số tệp/giây, tỉ lệ lỗi, thời gian xử lý trung bình) để giám sát, cảnh báo sớm.
- Khi hệ thống khởi động lại:
  - Đọc trạng thái từ kho meta-data.
  - Tự động xếp lại vào hàng đợi các tệp ở trạng thái `Pending` hoặc `Failed` nhưng còn trong ngưỡng retry.
  - Bỏ qua các tệp đã `Completed` để không xử lý lại.
- Có thể triển khai theo mô hình multi-instance: nhiều nút xử lý song song, tận dụng cơ chế hàng đợi phân tán để tự cân bằng tải và tăng khả năng chịu lỗi.