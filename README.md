# Aurora Hotel — đồ án quản lý khách sạn

Website chạy bằng **ASP.NET Core MVC (.NET 10), Razor, EF Core 10, SQL Server, Identity và Bootstrap 5**. Khu khách hàng và khu nhân viên dùng cùng cơ sở dữ liệu, service đặt phòng và service thanh toán. Không có dữ liệu nghiệp vụ cố định trong giao diện; dữ liệu minh họa được seed vào SQL Server.

**Mới tải từ GitHub? Đọc [Hướng dẫn cài đặt, chạy và sử dụng](HUONG_DAN_SU_DUNG.md)** — có cách tải ZIP/clone, cài .NET/LocalDB, lấy tài khoản demo, thử từng vai trò và xử lý lỗi thường gặp.

Repository: [Khoazxjee/BTLWEBTest](https://github.com/Khoazxjee/BTLWEBTest). GitHub lưu mã nguồn; để dùng đầy đủ chức năng, chạy local theo hướng dẫn hoặc triển khai lên hosting ASP.NET Core + SQL Server.

## Chạy nhanh trên Windows

Mở `AuroraHotel.sln` bằng Visual Studio hỗ trợ .NET 10, hoặc chạy PowerShell tại thư mục dự án:

```powershell
dotnet tool restore
dotnet restore
dotnet run --project src/AuroraHotel
```

Mở **http://localhost:5080**. Profile mặc định là Development: tự áp dụng migration và seed một lần vào `AuroraHotel_Development`, dùng SQL Server LocalDB với Windows Authentication. Máy đã được kiểm tra có .NET SDK 10.0.400 và SQL Server LocalDB.

Cách khởi động bằng một lệnh: `powershell -ExecutionPolicy Bypass -File scripts/run.ps1`. Script chỉ restore/build/chạy project, không thay đổi chính sách thực thi của máy.

## Tài khoản demo

| Email | Vai trò |
|---|---|
| `admin@aurora.local` | Admin |
| `letan@aurora.local` | Receptionist |
| `minh@aurora.local` | Customer |
| `linh@aurora.local` | Customer |

Mật khẩu ngẫu nhiên được tạo khi khởi chạy Development lần đầu và lưu tại **`src/AuroraHotel/App_Data/demo-accounts.txt`** (cùng mật khẩu cho bốn tài khoản minh họa). File này và `demo-password.txt` đã được loại khỏi Git. Không có mật khẩu demo cố định trong mã nguồn. Không xóa file mật khẩu rồi mong mật khẩu tài khoản cũ tự thay đổi: seed không ghi đè tài khoản có sẵn.

Chỉ seed tài khoản ở Development khi `SeedDemo=true`. Đăng ký công khai luôn tạo Customer. Identity lưu mật khẩu dưới dạng hash có salt. Admin đổi vai trò bằng trang Tài khoản & quyền; không cần chỉnh SQL thủ công.

## Cấu hình

Tên, tên thương hiệu viết ngắn, địa chỉ, điện thoại, email và múi giờ nằm trong `src/AuroraHotel/appsettings.json`, nhóm `Hotel`. Ảnh phòng được quản lý tại Danh mục phòng. Các tên loại phòng và nội dung giới thiệu phòng nằm trong database.

Muốn dùng SQL Server Express, tạo **`src/AuroraHotel/appsettings.Local.json`** (đã gitignore):

```json
{
  "ConnectionStrings": {
    "HotelDb": "Server=.\\SQLEXPRESS;Database=AuroraHotel_Development;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Đây là cấu hình phát triển trên máy cá nhân. Khi đưa lên máy chủ, dùng connection string qua biến môi trường `ConnectionStrings__HotelDb`, HTTPS và tài khoản SQL có quyền phù hợp; cấu hình `AllowedHosts` theo tên miền. Không đưa thông tin xác thực thật vào repository. Production không tự migrate và không tạo tài khoản demo.

## Cơ sở dữ liệu và SQL

* Migration: `src/AuroraHotel/Data/Migrations`.
* `docs/schema.sql`: script SQL dựng schema có kiểm tra lịch sử migration, chạy lặp an toàn trên SQL Server.
* `docs/seed-demo.sql`: bổ sung bộ dữ liệu nghiệp vụ minh họa tương đối theo ngày chạy, chỉ dùng database phát triển. Cần khởi chạy Development một lần để Identity tạo tài khoản an toàn; script không chứa mật khẩu hoặc hash tài khoản.
* `Data/SeedData.cs`: seed chính gồm 3 loại, 12 phòng, 4 dịch vụ, 4 tài khoản, 2 hồ sơ khách và 5 đơn nhiều trạng thái. Ngày demo căn cứ ngày của khách sạn tại lần khởi tạo.

Nếu không dùng cơ chế migrate tự động:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project src/AuroraHotel
dotnet ef migrations script --idempotent --project src/AuroraHotel --output docs/schema.sql
```

Không xóa database đang có dữ liệu để chạy demo lại. Có thể tạo đơn mới cho ngày hiện tại, chỉnh lịch đơn chưa nhận phòng, hoặc đổi tên database trong cấu hình Local để tạo bộ demo độc lập. Đơn đã lưu trú/thanh toán giữ nguyên lịch sử.

## Các phần đã triển khai

**Khách hàng:** trang chủ; tìm phòng theo ngày/số khách/loại/giá; sắp xếp/phân trang; tìm bằng AJAX gọi API khả dụng; chi tiết phòng; session giữ lựa chọn qua đăng nhập; đăng ký/đăng nhập/đăng xuất; tạo Pending; xem/hủy đơn thuộc mình; cập nhật email liên hệ riêng với email đăng nhập.

**Lễ tân:** lọc/tìm/phân trang đơn; chọn hoặc tạo hồ sơ khách trực tiếp; tạo/sửa/xác nhận/từ chối/hủy đơn; check-in; Room Board; cập nhật Clean/Dirty; thêm/sửa/xóa dịch vụ sử dụng; bảng kê; ghi một thanh toán đủ bằng tiền mặt/chuyển khoản; checkout; chứng từ có CSS in.

**Admin:** toàn bộ quyền lễ tân; loại phòng/phòng/dịch vụ CRUD; upload và kiểm tra ảnh; editor mô tả với lọc HTML server; quản lý khai thác phòng; tài khoản/phân quyền/khóa; dashboard theo kỳ từ Payments.

**Nghiệp vụ:** Pending không giữ chỗ; kiểm tra giao nhau nửa mở `[ArrivalDate, DepartureDate)`; giá chốt khi Confirmed; khóa SQL Server trong transaction; một folio/một phí phòng/một payment; checkout nguyên tử và chống lặp; giữ nguyên lịch sử giá.

## Kiểm tra

```powershell
dotnet build AuroraHotel.sln -c Release
dotnet test AuroraHotel.sln -c Release --logger 'trx;LogFileName=hotel-tests.trx' --results-directory TestResults
```

Test sử dụng **SQL Server LocalDB thật**, không thay bằng EF InMemory. Mỗi lần chạy tạo một database tên `AuroraHotel_Tests_<GUID>` và chỉ xóa database của lần kiểm thử đó. Xem kết quả đã chạy và giới hạn xác minh ở `docs/TESTING.md`.

## Tài liệu bàn giao

* [HUONG_DAN_SU_DUNG.md](HUONG_DAN_SU_DUNG.md): hướng dẫn dành cho người tải dự án từ GitHub lần đầu.
* `docs/HOSTING.md`: triển khai đầy đủ lên hosting miễn phí, GitHub, SQL remote và HTTPS.
* `docs/ARCHITECTURE.md`: cấu trúc, ERD, sơ đồ trang, vai trò, API, đồng thời và bảo mật.
* `docs/DEMO.md`: kịch bản 1.800.000 VND và hướng dẫn theo ba vai trò.
* `docs/REQUIREMENTS.md`: đối chiếu prompt và tiêu chí PDF môn học; phân công Hùng/Khoa/Trưởng.
* `docs/TESTING.md`: test case, kết quả thực tế, QA giao diện và giới hạn.
* `docs/screenshots`: ảnh màn hình đã kiểm tra.
* `docs/diagrams.drawio`: Use Case, ERD và hai wireframe khách/nhân viên, chỉnh sửa bằng diagrams.net/draw.io.
* `docs/ASSETS.md`: nguồn ảnh và thư viện.

Website dùng một ngôn ngữ tiếng Việt. Nhãn trạng thái/định dạng tập trung ở `Services/HotelClock.cs`; `DataAnnotations` ghi nhãn/lỗi bằng tiếng Việt. Ngày văn bản dùng dd/MM/yyyy, tiền VND, thời điểm UTC+7. Điều khiển ngày gốc của trình duyệt có thể hiển thị theo locale hệ điều hành. Phương án mở rộng `.resx`/localizer ghi trong tài liệu; không đặt nút chuyển ngôn ngữ giả.

## Phạm vi bàn giao

Đã triển khai và kiểm tra cục bộ trên Windows + SQL Server. Chưa triển khai lên IIS/hosting công khai và chưa kiểm tra máy in vật lý. Hosting Sites chạy Workers không phù hợp với backend ASP.NET + SQL Server, nên giữ đúng nền tảng đã chọn và bàn giao source chạy cục bộ. Không có cổng thanh toán trực tuyến, hóa đơn điện tử, đa chi nhánh, OTA, cọc, thuế hoặc hoàn tiền trong phiên bản này.

Tài liệu môn học còn yêu cầu báo cáo 15–35 trang và thuyết trình. Các tài liệu kỹ thuật đi kèm là nội dung đầu vào cho báo cáo; chưa phải bản báo cáo có bìa/tên trường/lớp/giảng viên. Không tạo commit giả danh thành viên hay PR/review thay người thật.
