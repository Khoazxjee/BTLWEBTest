using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuroraHotel.Models;

public static class Roles
{
    public const string Admin = "Admin", Receptionist = "Receptionist", Customer = "Customer";
    public const string Staff = Admin + "," + Receptionist;
}
public enum ReservationStatus { Pending, Confirmed, CheckedIn, CheckedOut, Rejected, Cancelled }
public enum HousekeepingStatus { Clean, Dirty }
public enum OperationalStatus { Active, OutOfOrder }
public enum FolioStatus { Open, Paid, Closed }
public enum ItemKind { Room, Service }
public enum PaymentMethod { Cash, BankTransfer }
public enum ReservationSource { Website, FrontDesk }
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)] public string FullName { get; set; } = "";
}
public class RoomType
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";
    public int Capacity { get; set; }
    [MaxLength(2000)] public string Description { get; set; } = "";
    public List<Room> Rooms { get; set; } = [];
}
public class Room
{
    public int Id { get; set; }
    [MaxLength(20)] public string RoomNumber { get; set; } = "";
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
    public decimal NightlyRate { get; set; }
    [MaxLength(500)] public string ImagePath { get; set; } = "/images/room-deluxe.jpg";
    [MaxLength(12000)] public string Description { get; set; } = "";
    [MaxLength(1000)] public string Amenities { get; set; } = "";
    public HousekeepingStatus HousekeepingStatus { get; set; }
    public OperationalStatus OperationalStatus { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public List<Reservation> Reservations { get; set; } = [];
}
public class GuestProfile
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    [MaxLength(100)] public string FullName { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(2000)] public string Notes { get; set; } = "";
    public List<Reservation> Reservations { get; set; } = [];
}
public class Reservation
{
    public int Id { get; set; }
    [MaxLength(30)] public string ConfirmationCode { get; set; } = "";
    public int GuestProfileId { get; set; }
    public GuestProfile GuestProfile { get; set; } = null!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public DateOnly ArrivalDate { get; set; }
    public DateOnly DepartureDate { get; set; }
    public int GuestCount { get; set; }
    public decimal NightlyRateSnapshot { get; set; }
    public ReservationStatus Status { get; set; }
    public ReservationSource Source { get; set; }
    public string CreatedByUserId { get; set; } = "";
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    [MaxLength(2000)] public string Notes { get; set; } = "";
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public Folio? Folio { get; set; }
    public int Nights => DepartureDate.DayNumber - ArrivalDate.DayNumber;
    public decimal RoomTotal => Nights * NightlyRateSnapshot;
}
public class HotelService
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";
    [MaxLength(30)] public string Unit { get; set; } = "lần";
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
public class Folio
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public FolioStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public List<FolioItem> Items { get; set; } = [];
    public Payment? Payment { get; set; }
    public decimal Total => Items.Sum(x => x.Quantity * x.UnitPriceSnapshot);
    public decimal Paid => Payment?.Amount ?? 0;
    public decimal Balance => Total - Paid;
}
public class FolioItem
{
    public int Id { get; set; }
    public int FolioId { get; set; }
    public Folio Folio { get; set; } = null!;
    public ItemKind Kind { get; set; }
    public int? ServiceId { get; set; }
    public HotelService? Service { get; set; }
    [MaxLength(300)] public string DescriptionSnapshot { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal Total => Quantity * UnitPriceSnapshot;
}
public class Payment
{
    public int Id { get; set; }
    public int FolioId { get; set; }
    public Folio Folio { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public string ReceivedByUserId { get; set; } = "";
    public ApplicationUser ReceivedByUser { get; set; } = null!;
}
