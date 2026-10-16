# Rà soát font và bố cục — 15/10/2026

Đã sửa trực tiếp giao diện đang phục vụ tại `http://localhost:5080/`. Thay đổi nằm ở `wwwroot/css/site.css` và bộ font cục bộ; giữ nguyên nghiệp vụ và dữ liệu đặt phòng.

## Các vấn đề đã xử lý

| Vấn đề | Điều chỉnh |
|---|---|
| Dấu tiếng Việt ở tiêu đề serif bị rời, hiển thị khác nhau theo font hệ điều hành | Dùng Lora cho tiêu đề và Be Vietnam Pro cho nội dung, biểu mẫu, quản trị; lưu WOFF2 và giấy phép trong dự án |
| Chữ phụ, nhãn nhập liệu, bảng và sidebar quá nhỏ | Thống nhất cỡ chữ, độ đậm, màu chữ phụ và chiều cao dòng; nội dung bảng 14px, ô nhập 15px, ô nhập trên điện thoại 16px |
| Hero và phần giới thiệu chiếm nhiều khoảng trống | Giảm chiều cao/padding ở desktop, điều chỉnh tiêu đề theo chiều rộng điện thoại |
| Chân thẻ phòng không thẳng hàng khi mô tả dài ngắn khác nhau | Thẻ dùng flex theo cột, phần giá và nút chi tiết cùng nằm ở đáy |
| Biểu mẫu và ngày tháng bị chật trên màn hình nhỏ | Xếp một cột dưới 480px; trang đăng ký giữ các trường đủ rộng |
| Các ô thống kê, bảng kê và biểu đồ bị ép ở độ rộng trung gian | Thống kê chuyển 4/2/1 cột theo breakpoint; bảng kê và dashboard chuyển một cột khi thiếu chỗ |
| Thẻ tình trạng phòng nhỏ, số phòng chưa nổi bật | Grid tự điều chỉnh theo chiều rộng, tăng số phòng và cỡ chữ điều khiển |
| Nút lưu và lịch sử trong hồ sơ sát nhau | Thêm khoảng cách; nút trải hết chiều rộng ở điện thoại |
| Bảng dài làm màn hình nhỏ khó đọc | Giữ cỡ chữ, cho cuộn ngang trong khung bảng; không kéo rộng toàn trang |
| Quy tắc in dễ bị CSS responsive ghi đè | Gom quy tắc in xuống cuối stylesheet, bỏ sidebar và khoảng lề workspace khi in |

## Phạm vi xác minh

- 29 đường dẫn khác nhau thuộc các nhóm: trang chủ, danh sách/chi tiết phòng, đăng nhập/đăng ký, yêu cầu đặt phòng, lịch sử/chi tiết đơn, hồ sơ cá nhân, dashboard, lễ tân, tạo/sửa/chi tiết đặt phòng, bảng phòng, danh sách/thêm/sửa/chi tiết khách, phòng/loại phòng/dịch vụ và biểu mẫu sửa, tài khoản, bảng kê, chứng từ.
- 79 lượt đo DOM ở các chiều rộng nội dung thực tế từ 303 đến 1600 CSS px. Không phát hiện tràn ngang toàn trang, thành phần chính vượt khung, hoặc ảnh đã tải bị lỗi trong các lượt này. Dữ liệu đo lưu tại `artifacts/ui-review-audit.json` trên máy kiểm tra.
- Xem ảnh trực quan các bố cục đại diện và kiểm tra menu khách hàng/menu nhân viên trên điện thoại; menu mở và điều hướng được.
- Cả 6 file font trả HTTP 200. Kiểm tra cmap xác nhận đủ bộ nguyên âm có dấu và Đ/đ tiếng Việt, cả chữ thường lẫn chữ hoa. 45 file view/stylesheet đọc được UTF-8, không có ký tự thay thế U+FFFD.
- Chạy lại bộ kiểm thử hiện có: **18 PASS, 0 FAIL, 0 SKIP**. Lệnh: `dotnet test tests/AuroraHotel.Tests/AuroraHotel.Tests.csproj -c Release --no-build --logger "trx;LogFileName=ui-review-regression.trx" --results-directory artifacts/test-results`. Không sửa mã C#/Razor trong lượt chỉnh giao diện này. Kiểm thử dùng database riêng.

Ảnh minh chứng: [đăng nhập desktop](screenshots/ui-review/login-desktop.png), [dashboard desktop](screenshots/ui-review/dashboard-desktop.png), [dashboard điện thoại](screenshots/ui-review/dashboard-mobile.png), [trang chủ điện thoại](screenshots/ui-review/home-mobile.png).

Kiểm tra responsive dùng trình duyệt tích hợp với viewport giả lập; độ rộng CSS thực tế đã được ghi lại vì localhost đang có mức zoom riêng. Chưa xác minh Safari/Firefox, điện thoại thật hoặc bản in giấy. Kết quả này xác nhận các trạng thái và dữ liệu đã kiểm tra, không phải mọi tổ hợp nội dung có thể nhập.
