using System.ComponentModel.DataAnnotations;
using AuroraHotel.Models;

namespace AuroraHotel.ViewModels;
public class LoginInput
{
    [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), Display(Name = "Email đăng nhập")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu."), DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = "";
    [Display(Name = "Ghi nhớ đăng nhập")] public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
public class RegisterInput
{
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, MinimumLength = 2, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Họ và tên")] public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập {0}."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(200, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Email đăng nhập")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập {0}."), Phone(ErrorMessage = "Số điện thoại chưa đúng định dạng."), StringLength(30, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Số điện thoại")] public string Phone { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, MinimumLength = 8, ErrorMessage = "{0} có độ dài không hợp lệ."), DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = "";
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận chưa khớp."), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")] public string ConfirmPassword { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class GuestInput
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, MinimumLength = 2, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Họ và tên")] public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập {0}."), Phone(ErrorMessage = "Số điện thoại chưa đúng định dạng."), StringLength(30, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Số điện thoại")] public string Phone { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập {0}."), EmailAddress(ErrorMessage = "Email chưa đúng định dạng."), StringLength(200, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Email liên hệ")] public string Email { get; set; } = "";
    [StringLength(2000, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Ghi chú")] public string? Notes { get; set; }
}
public class StayInput
{
    [Range(1, int.MaxValue, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Phòng")] public int RoomId { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập {0}."), DataType(DataType.Date), Display(Name = "Ngày nhận phòng")] public DateOnly ArrivalDate { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập {0}."), DataType(DataType.Date), Display(Name = "Ngày trả phòng")] public DateOnly DepartureDate { get; set; }
    [Range(1, 20, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Số khách")] public int GuestCount { get; set; } = 2;
    [StringLength(2000, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Ghi chú cho khách sạn")] public string? Notes { get; set; }
}
public class BookingInput : StayInput
{
    [Range(1, int.MaxValue, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Khách hàng")] public int GuestProfileId { get; set; }
}
public class SearchInput
{
    [DataType(DataType.Date)] public DateOnly? ArrivalDate { get; set; }
    [DataType(DataType.Date)] public DateOnly? DepartureDate { get; set; }
    [Range(1, 20, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public int GuestCount { get; set; } = 2;
    public int? RoomTypeId { get; set; }
    [Range(0, 1000000000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public decimal? MinPrice { get; set; }
    [Range(0, 1000000000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "price";
    [Range(1, 100000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public int Page { get; set; } = 1;
}
public class RoomInput
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(20, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Số phòng")] public string RoomNumber { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Loại phòng")] public int RoomTypeId { get; set; }
    [Range(1, 1000000000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Giá mỗi đêm (VND)")] public decimal NightlyRate { get; set; } = 800000;
    [StringLength(12000, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Mô tả phòng")] public string? Description { get; set; }
    [StringLength(1000, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Tiện nghi (ngăn cách bằng dấu phẩy)")] public string? Amenities { get; set; }
    [Display(Name = "Ảnh phòng (JPEG, PNG, WebP; tối đa 5 MB)")] public IFormFile? Image { get; set; }
    public string? CurrentImage { get; set; }
}
public class RoomTypeInput
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Tên loại phòng")] public string Name { get; set; } = "";
    [Range(1, 20, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Sức chứa")] public int Capacity { get; set; } = 2;
    [StringLength(2000, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Mô tả")] public string? Description { get; set; }
}
public class ServiceInput
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Tên dịch vụ")] public string Name { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(30, ErrorMessage = "{0} có độ dài không hợp lệ."), Display(Name = "Đơn vị tính")] public string Unit { get; set; } = "lần";
    [Range(0, 1000000000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép."), Display(Name = "Đơn giá (VND)")] public decimal UnitPrice { get; set; }
    [Display(Name = "Đang cung cấp")] public bool IsActive { get; set; } = true;
}
public class ChargeInput
{
    [Range(1, int.MaxValue, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public int ServiceId { get; set; }
    [Range(1, 1000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public int Quantity { get; set; } = 1;
}
public class PaymentInput
{
    [Range(1, 1000000000000, ErrorMessage = "{0} nằm ngoài phạm vi cho phép.")] public decimal Amount { get; set; }
    [EnumDataType(typeof(PaymentMethod))] public PaymentMethod Method { get; set; }
}
public record PageResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize = 9)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling((double)Total / PageSize));
}
public record SearchPage(SearchInput Search, PageResult<Room> Results, List<RoomType> Types);
public record BookingPage(StayInput Input, Room Room, GuestProfile Guest);
public record DashboardModel(DateOnly From, DateOnly To, int Occupied, int Dirty, int Available, decimal Revenue, int PaymentCount, Dictionary<ReservationStatus,int> Counts, List<Reservation> Recent);
