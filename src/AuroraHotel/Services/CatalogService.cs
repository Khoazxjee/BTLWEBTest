using AuroraHotel.Data;
using AuroraHotel.Models;
using AuroraHotel.ViewModels;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace AuroraHotel.Services;
public class CatalogService(HotelDbContext db, HotelOperations operations, IWebHostEnvironment environment)
{
    public static string Sanitize(string? html)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[] { "p", "br", "strong", "b", "em", "i", "ul", "ol", "li", "h3", "h4" }) sanitizer.AllowedTags.Add(tag);
        sanitizer.AllowedAttributes.Clear(); sanitizer.AllowedCssProperties.Clear();
        return sanitizer.Sanitize(html ?? "");
    }
    public async Task<string?> Upload(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) throw new BusinessException("Ảnh không được lớn hơn 5 MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension)) throw new BusinessException("Chỉ nhận ảnh JPEG, PNG hoặc WebP.");
        try {
            await using var stream = file.OpenReadStream();
            using var codec = SKCodec.Create(stream);
            if (codec == null || codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
                throw new BusinessException("Nội dung tệp không phải JPEG, PNG hoặc WebP hợp lệ.");
            var info = codec.Info;
            if (info.Width > 8000 || info.Height > 8000 || (long)info.Width * info.Height > 32000000)
                throw new BusinessException("Ảnh quá lớn; tối đa 8.000 px mỗi chiều và 32 megapixel.");
            using var bitmap = SKBitmap.Decode(codec);
            if (bitmap == null) throw new BusinessException("Tệp ảnh bị hỏng.");
            using var image = SKImage.FromBitmap(bitmap);
            var name = Guid.NewGuid().ToString("N") + ".webp";
            var folder = Path.Combine(environment.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            // Decode then re-encode: user content and filenames never become executable files.
            using var data = image.Encode(SKEncodedImageFormat.Webp, 85);
            await File.WriteAllBytesAsync(Path.Combine(folder, name), data.ToArray());
            return "/uploads/" + name;
        } catch (ArgumentException) { throw new BusinessException("Không đọc được ảnh. Vui lòng chọn tệp ảnh hợp lệ."); }
    }
    public async Task<int> SaveRoom(RoomInput input)
    {
        if (input.NightlyRate <= 0 || input.NightlyRate != decimal.Truncate(input.NightlyRate)) throw new BusinessException("Giá phòng phải là số nguyên VND lớn hơn 0.");
        var image = await Upload(input.Image);
        return await operations.Atomic(async () => {
            if (await db.Rooms.AnyAsync(x => x.RoomNumber == input.RoomNumber.Trim() && x.Id != input.Id))
                throw new BusinessException("Số phòng đã tồn tại.");
            var type = await db.RoomTypes.FindAsync(input.RoomTypeId) ?? throw new BusinessException("Loại phòng không tồn tại.");
            var room = input.Id == 0 ? new Room() : await db.Rooms.FindAsync(input.Id) ?? throw new BusinessException("Không tìm thấy phòng.");
            if (await db.Reservations.AnyAsync(x => x.RoomId == room.Id && (x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.CheckedIn) && x.GuestCount > type.Capacity))
                throw new BusinessException("Loại phòng mới không đủ sức chứa cho các đơn đã xác nhận.");
            room.RoomNumber = input.RoomNumber.Trim(); room.RoomTypeId = type.Id; room.NightlyRate = input.NightlyRate;
            room.Description = Sanitize(input.Description); room.Amenities = input.Amenities ?? "";
            if (image != null) room.ImagePath = image;
            if (input.Id == 0) db.Rooms.Add(room);
            await db.SaveChangesAsync(); return room.Id;
        });
    }
    public Task DeleteRoom(int id) => operations.Atomic(async () => {
        if (await db.Reservations.AnyAsync(x => x.RoomId == id)) throw new BusinessException("Phòng đã có lịch sử đặt. Hãy dùng trạng thái ngừng sử dụng.");
        db.Rooms.Remove(await db.Rooms.FindAsync(id) ?? throw new BusinessException("Không tìm thấy phòng."));
    });
    public Task SaveType(RoomTypeInput input) => operations.Atomic(async () => {
        var type = input.Id == 0 ? new RoomType() : await db.RoomTypes.FindAsync(input.Id) ?? throw new BusinessException("Không tìm thấy loại phòng.");
        if (input.Id != 0 && await db.Reservations.AnyAsync(x => x.Room.RoomTypeId == input.Id &&
            (x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.CheckedIn) && x.GuestCount > input.Capacity))
            throw new BusinessException("Sức chứa mới không phù hợp với đơn đã xác nhận.");
        type.Name = input.Name.Trim(); type.Capacity = input.Capacity; type.Description = input.Description ?? "";
        if (input.Id == 0) db.RoomTypes.Add(type);
    });
    public Task DeleteType(int id) => operations.Atomic(async () => {
        if (await db.Rooms.AnyAsync(x => x.RoomTypeId == id)) throw new BusinessException("Loại phòng đang được sử dụng. Không thể xóa.");
        db.RoomTypes.Remove(await db.RoomTypes.FindAsync(id) ?? throw new BusinessException("Không tìm thấy loại phòng."));
    });
    public Task<int> SaveService(ServiceInput input) => operations.Atomic(async () => {
        if (input.UnitPrice < 0 || input.UnitPrice != decimal.Truncate(input.UnitPrice)) throw new BusinessException("Đơn giá dịch vụ phải là số nguyên VND không âm.");
        var service = input.Id == 0 ? new HotelService() : await db.Services.FindAsync(input.Id) ?? throw new BusinessException("Không tìm thấy dịch vụ.");
        service.Name = input.Name.Trim(); service.Unit = input.Unit.Trim(); service.UnitPrice = input.UnitPrice; service.IsActive = input.IsActive;
        if (input.Id == 0) db.Services.Add(service);
        await db.SaveChangesAsync(); return service.Id;
    });
    public Task DeleteService(int id) => operations.Atomic(async () => {
        var service = await db.Services.FindAsync(id) ?? throw new BusinessException("Không tìm thấy dịch vụ.");
        if (await db.FolioItems.AnyAsync(x => x.ServiceId == id)) service.IsActive = false;
        else db.Services.Remove(service);
    });
    public async Task<PageResult<HotelService>> Services(string? q, int page, bool activeOnly = false)
    {
        var query = db.Services.AsNoTracking().Where(x => !activeOnly || x.IsActive);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Name.Contains(q));
        var total = await query.CountAsync(); page = Math.Clamp(page, 1, Math.Max(1, (total + 11) / 12));
        return new(await query.OrderBy(x => x.Name).Skip((page - 1) * 12).Take(12).ToListAsync(), total, page, 12);
    }
}
