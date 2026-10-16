# Kết quả kiểm thử

Lần kiểm tra bàn giao: **14/10/2026, múi giờ Việt Nam**. Môi trường Windows, .NET SDK 10.0.400, ASP.NET Core/EF/Identity 10.0.11, SQL Server LocalDB. Kiểm thử trực tiếp trên SQL Server, không dùng SQLite/InMemory thay cho kiểm tra transaction/khóa.

## Kết quả tự động

**Cập nhật giao diện 15/10/2026:** đã rà soát font tiếng Việt, khoảng cách và responsive trên 29 đường dẫn, đồng thời chạy lại bộ kiểm thử hiện có thành công **18/18**. Chi tiết và phạm vi xác minh: [UI-REVIEW.md](UI-REVIEW.md). Các kết quả build, audit package và kiểm thử nghiệp vụ bên dưới thuộc lần bàn giao 14/10/2026.

`dotnet build AuroraHotel.sln -c Release`: **PASS — 0 lỗi, 0 cảnh báo**.

`dotnet test AuroraHotel.sln -c Release --logger 'trx;LogFileName=hotel-tests.trx' --results-directory TestResults`: **18/18 PASS, 0 FAIL, 0 SKIP**. Kết quả chi tiết tại `TestResults/hotel-tests.trx` trên máy chạy. Bản tóm tắt được lưu trong `docs/test-results.txt`. Test khởi tạo và hủy database riêng tên AuroraHotel_Tests_GUID / AuroraHotel_HostingTests_GUID, không xóa dữ liệu demo.

`dotnet list src/AuroraHotel/AuroraHotel.csproj package --vulnerable --include-transitive`: không báo package có lỗ hổng theo nguồn NuGet tại thời điểm kiểm tra. Đây không thay thế một cuộc kiểm toán bảo mật.

| Test case | Đầu vào / mục tiêu | Mong đợi | Thực tế |
|---|---|---|---|
| TC01 End-to-end chính | 2 đêm × 800000, dịch vụ 2 × 100000; đổi giá catalog; 2 request thu tiền đồng thời | Tổng 1800000; giữ giá; 1 Payment; Closed; Dirty; retry không đánh bẩn lại phòng đã dọn | PASS |
| TC02 Xác nhận đồng thời | 2 Pending cùng phòng/ngày, hai DbContext SQL riêng | Chính xác 1 Confirmed; đơn còn lại bị chặn | PASS |
| TC03 Khoảng ngày/Dirty/Pending | Đơn [5,7), [7,9), [6,8); phòng Dirty | Hai đơn liền kề hợp lệ; chồng bị chặn; Dirty vẫn tìm được kỳ sau | PASS |
| TC04 Dữ liệu đặt phòng | Ngày quá khứ/bằng nhau, vượt sức chứa, OutOfOrder; đổi sang phòng trùng | BusinessException, không lưu thay đổi sai | PASS |
| TC05 Check-in | Dirty, ngày nhận tương lai, repeat check-in, chuyển OutOfOrder lúc đang ở | Chặn sai; một folio duy nhất; không hủy đơn đang ở | PASS |
| TC06 Hủy/ownership | Khách B hủy đơn A; A hủy trước ngày đến/đúng ngày; chọn GuestId B | Chặn B; cho hủy hợp lệ; không cho hủy đúng ngày đến; không dùng hồ sơ B | PASS |
| TC07 Dòng dịch vụ | Sửa SL 1→2; SL0; xóa dòng phòng; xóa dịch vụ | Snapshot giữ nguyên; chặn SL0/xóa dòng phòng; tổng tự tính lại | PASS |
| TC08 Lịch sử/XSS | Xóa phòng có đơn/loại có phòng/dịch vụ đã dùng; HTML script/onerror | Chặn xóa room/type; service inactive; HTML nguy hiểm bị loại | PASS |
| TC09 Trang/quyền | GET trang công khai; Customer gọi quản trị; Receptionist gọi Admin; Admin mở catalog | Trang đúng quyền 200; API 403; trang quản trị redirect access denied | PASS |
| TC10 URL/CSRF | B truy cập/hủy ID đơn A; POST không token | 404 với ownership; 400 với CSRF | PASS |
| TC11 Session/overposting | Chọn phòng chưa login; login; gửi giá=1 và GuestId người khác | Giữ lựa chọn; đơn Pending giá server800000, khách từ claim | PASS |
| TC12 REST CRUD | Admin tạo/sửa/xóa service; thiếu token; số âm; Receptionist DELETE | 201/200/204; 400 invalid/CSRF; 403 role; dữ liệu thật | PASS |
| TC13 Profile/Identity | Gửi Id hồ sơ B; đổi email liên hệ A thành email B | Chỉ hồ sơ A đổi; email đăng nhập/quyền sở hữu không đổi | PASS |
| TC14 Upload | Script giả .jpg; ảnh PNG hợp lệ tên path traversal; >5MB | Chặn giả/oversize; decode→WebP tên ngẫu nhiên | PASS |
| TC15 Quản trị/đăng ký | Admin tự khóa/hạ quyền; đăng ký gửi Role=Admin và URL ngoài | Chặn tự khóa; người mới chỉ Customer; redirect nội bộ | PASS |
| TC16 Filter/paging/back link | Loại+giá+số khách; page999; ngày ngược; backURL nội bộ/ngoài | Đúng kết quả/trang hợp lệ; 400 ngày sai; giữ filter, chặn URL ngoài | PASS |
| TC17 SQL script | Chạy schema idempotent và seed hai lần sau bootstrap Identity | Không nhân đôi 3 đơn SQL; folio đúng 1800000 | PASS |
| TC18 Hosting | Production không tự seed; CLI kiểm tra tên DB/mật khẩu; DB mới có đủ demo; mỗi tài khoản mật khẩu riêng; chạy lại; HTTPS và cookie | Không seed ngầm; chặn sai trước khi migrate; tạo đủ 4 user/12 phòng/5 đơn; không ghi đè; cookie Secure; không có endpoint setup | PASS |

TC01 còn kiểm tra MVC chi tiết đơn, folio và chứng từ trả HTTP 200 sau checkout. TC09 kiểm tra các màn hình danh sách và biểu mẫu chính của từng khu vực.

## Kiểm tra bằng trình duyệt

| Luồng / giao diện | Kết quả đã quan sát |
|---|---|
| Trang chủ desktop | Ảnh, header, hero, form tìm phòng và dữ liệu phòng từ database hiển thị đúng |
| Tìm kiếm | AJAX giá tối đa900000 trả 4 phòng Deluxe tại thời điểm QA; URL cập nhật, không reload toàn trang |
| Đăng nhập Admin | Vào dashboard, sidebar đúng quyền, số liệu đọc từ database |
| Xác nhận / nhận phòng | AU-DEMO-PENDING → Confirmed → CheckedIn; 2 đêm giá800000 |
| Ghi dịch vụ / thu tiền | 2 bữa sáng100000; tổng1800000; xác nhận thu tiền; UI khóa các khoản phí |
| Checkout / housekeeping | Folio Closed; phòng102 trống/Dirty; cập nhật Clean thành công |
| Dashboard sau thu | Tổng tiền tháng từ2400000 lên4200000; số Payment từ1 lên2 |
| Chứng từ | Thông tin khách/phòng/kỳ ở/từng dòng phí/người thu/method/time hiển thị đúng |
| Mobile | Kiểm tra viewport điện thoại390×844 (CSS viewport của môi trường có zoom); menu thu gọn, form2 cột, cards1 cột, dashboard2 cột; không tràn ngang toàn trang |
| Desktop | Kiểm tra viewport1440×1000; sidebar, stats, bảng và card phòng |
| Ảnh | Ảnh tĩnh local tải được; ảnh dưới fold dùng lazy loading; fallback logo khi ảnh lỗi |

Ảnh minh chứng: `screenshots/home-desktop.jpg`, `home-mobile.jpg`, `dashboard-desktop.jpg`, `dashboard-mobile.jpg`, `folio-completed.jpg`.

## Lỗi phát hiện và sửa

* JS xử lý submit đọc `form.method` bị field `id=method` của thanh toán che thuộc tính DOM. Đã dùng `form.getAttribute('method')`; backend vẫn ghi nhận đúng và checkout được xác minh sau sửa.
* Link quay lại danh sách từ chi tiết ban đầu chỉ giữ ngày/số khách. Đã thêm returnURL nội bộ để giữ bộ lọc giá/loại/sắp xếp/trang và thêm TC16.
* Thư viện xử lý ảnh ban đầu có giới hạn license build/vulnerability. Đã thay bằng SkiaSharp, kiểm tra upload thật, build Release và audit NuGet không có cảnh báo.
* Khởi động thử trước khi biên dịch migration mới khiến schema chưa được áp dụng. Đã build migration, chạy lại thành công và bàn giao run script có build thông thường.

## Giới hạn xác minh

* Chưa kiểm thử trên thiết bị điện thoại thật, Safari/Firefox, máy in vật lý hoặc máy chủ IIS/hosting. QA responsive dùng trình duyệt tích hợp; chứng từ có CSS `@media print`, chưa xác minh giấy in thực tế.
* Chưa chạy load test nhiều giờ/penetration test. Khóa giao dịch hiện tối ưu tính đơn giản và nhất quán cho một khách sạn; chưa đo throughput tải lớn.
* Không cần/không có tích hợp ngân hàng, cổng thanh toán, email, Oracle hoặc hóa đơn điện tử.
* Tài liệu report 15–35 trang, tên nhóm/lớp/giảng viên và việc review PR thật cần nhóm hoàn thiện theo quy định môn học.
