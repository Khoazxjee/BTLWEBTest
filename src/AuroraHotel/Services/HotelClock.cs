using System.Globalization;
using AuroraHotel.Models;

namespace AuroraHotel.Services;
public class HotelOptions
{
    public string Name { get; set; } = "Aurora Hotel";
    public string BrandName { get; set; } = "AURORA";
    public string Address { get; set; } = "28 Đường Bình Minh, Đà Nẵng (địa chỉ minh họa)";
    public string Phone { get; set; } = "0236 000 0000";
    public string Email { get; set; } = "hello@aurora.example";
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
}
public interface IHotelClock { DateTimeOffset Now { get; } DateOnly Today { get; } }
public class HotelClock(Microsoft.Extensions.Options.IOptions<HotelOptions> options) : IHotelClock
{
    public DateTimeOffset Now => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone));
    public DateOnly Today => DateOnly.FromDateTime(Now.DateTime);
}
public static class Ui
{
    public static string Money(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VND";
    public static string Date(DateOnly value) => value.ToString("dd/MM/yyyy");
    public static string Time(DateTimeOffset value) => value.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm");
    public static string Label(this ReservationStatus value) => value switch {
        ReservationStatus.Pending => "Chờ xác nhận", ReservationStatus.Confirmed => "Đã xác nhận",
        ReservationStatus.CheckedIn => "Đang lưu trú", ReservationStatus.CheckedOut => "Đã trả phòng",
        ReservationStatus.Cancelled => "Đã hủy", _ => "Đã từ chối" };
    public static string Label(this FolioStatus value) => value switch { FolioStatus.Open => "Chưa thanh toán", FolioStatus.Paid => "Đã thanh toán", _ => "Đã đóng" };
    public static string Label(this PaymentMethod value) => value == PaymentMethod.Cash ? "Tiền mặt" : "Chuyển khoản";
}
public class BusinessException(string message) : Exception(message);
