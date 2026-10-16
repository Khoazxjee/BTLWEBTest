# Đối chiếu yêu cầu và phân công

Nguồn chính: `references/HotelRequirements.txt` do người dùng cung cấp. Người dùng xác nhận **ASP.NET Core MVC**, không dùng MVC 5. Nguồn bổ sung: [YeuCauBaiTapLon.pdf trên Google Drive](https://drive.google.com/file/d/1NQPNslhcTx0k3nXyhvn6nrl0muoT6KDw/view), đã đọc trong trình xem Drive ngày 14/10/2026; chủ file tắt tải xuống nên không lưu một bản PDF giả/không hợp lệ trong repo.

## Yêu cầu kỹ thuật môn học

| Tiêu chí PDF | Vị trí thực hiện |
|---|---|
| ASP.NET MVC; ít nhất 2 nhóm quyền | Core MVC 10, 3 role Identity; Controllers/Areas/Staff |
| Framework frontend, responsive | Bootstrap 5 local + CSS media queries; menu thu gọn, bảng cuộn |
| Paging, Filtering, AJAX | Rooms/Index + site.js → `/api/rooms/availability`; bảng Staff |
| Thêm/sửa/xóa bảng cần thiết | Phòng, loại phòng, dịch vụ; giữ dữ liệu đã có lịch sử |
| Session và cookie | Bookings/Select giữ kỳ ở; Identity application cookie |
| Validation và Annotation | ViewModels/Inputs.cs, ModelState, service validation, DB check constraints |
| RESTful API / Service | GET/POST/PUT/DELETE `/api/services`; API khả dụng phòng |
| Upload ảnh | CatalogService.Upload, giới hạn định dạng/kích thước, decode/re-encode |
| Editor textarea | Toolbar/ContentEditable đồng bộ textarea; allowlist HTML phía server |
| Kiểm thử | xUnit + WebApplicationFactory + SQL Server LocalDB + QA browser |
| Security, quốc tế hóa | Identity/CSRF/ownership/ORM/encode/CSP; vi-VN và phương án resx |
| Mã nguồn và hướng dẫn | Solution, README, tài liệu kiến trúc và demo |
| SQL script | docs/schema.sql và docs/seed-demo.sql |
| Báo cáo 15–35 trang | Chưa tạo bản báo cáo định dạng nộp; docs cung cấp nội dung kỹ thuật để biên soạn |
| Báo cáo, thuyết trình cuối kỳ | Công việc của nhóm; không giả lập việc thuyết trình hoặc review |

## Sở hữu phần việc theo prompt

| Thành viên | Phần code chính | Bàn giao/đầu mối |
|---|---|---|
| Hùng | AccountController; Catalog Room/Type; Board; Users; Identity/DbContext | Tài khoản, quyền, phòng, upload/editor, tình trạng phòng |
| Khoa | Home/Rooms/Bookings; GuestInput; GuestProfiles; Search/Create/Confirm/Update | Tìm/đặt phòng, profile, AJAX, guest workflow |
| Trưởng | FrontDesk, HotelOperations phí/payment/checkout; Catalog Service; ServicesApi; Dashboard | Lưu trú, dịch vụ, folio, thanh toán, báo cáo |

Schema và service dùng chung phải thống nhất trước khi thay đổi song song. Gợi ý nhánh trong prompt: feat/auth, feat/rooms, feat/housekeeping, feat/room-search, feat/bookings, feat/front-desk, feat/services, feat/folios-payments. Hùng review Khoa; Khoa review Trưởng; Trưởng review Hùng. Dùng tài khoản Git thật, một người khác review trước khi merge; không có commit/PR giả mạo thành viên trong bản bàn giao.

## Gợi ý kế hoạch 7 ngày để nhóm tiếp nhận

1. Chạy solution, schema, Identity và đọc luồng service chung.
2. Hùng kiểm tra phòng/loại/quyền; Khoa kiểm tra tìm phòng; Trưởng kiểm tra giao diện lễ tân.
3. Tập trung Pending/Confirmed, sửa lịch và đồng thời.
4. Nhận phòng, folio, dịch vụ và lịch sử giá.
5. Thu tiền, checkout, in chứng từ và dashboard.
6. Chạy toàn bộ test, kiểm tra responsive và bổ sung ảnh minh chứng của nhóm.
7. Hoàn thiện báo cáo 15–35 trang, use case/ERD/sitemap/wireframe và bài thuyết trình; đóng gói theo mã nhóm/lớp do giảng viên quy định.

Không tự mở rộng cổng thanh toán, đa chi nhánh, OTA, dynamic pricing, kế toán, đặt cọc, thuế hoặc hoàn tiền. OPERA chỉ là tham khảo nghiệp vụ; không có API/cấu hình Oracle.
