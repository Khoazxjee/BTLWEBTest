using AuroraHotel.Data;
using AuroraHotel.Models;
using AuroraHotel.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AuroraHotel.Services;

// A transaction-scoped SQL Server application lock serializes mutations for this one-hotel project.
// It works across HTTP requests/processes, unlike an in-memory lock. Every domain writer uses it.
public class HotelOperations(HotelDbContext db, IHotelClock clock)
{
    public async Task<T> Atomic<T>(Func<Task<T>> action)
    {
        db.ChangeTracker.Clear();
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource=N'AuroraHotel:operations',
                @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000;
            IF @result < 0 THROW 51000, 'Hotel is busy; retry the operation.', 1;
            """);
        var result = await action();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return result;
    }
    public Task Atomic(Func<Task> action) => Atomic(async () => { await action(); return true; });

    public void ValidateStay(DateOnly arrival, DateOnly departure, int guests, int capacity)
    {
        if (arrival < clock.Today) throw new BusinessException("Ngày nhận phòng không được ở trong quá khứ.");
        if (departure <= arrival) throw new BusinessException("Ngày trả phòng phải sau ngày nhận phòng.");
        if (departure.DayNumber - arrival.DayNumber > 365) throw new BusinessException("Mỗi đơn hỗ trợ tối đa 365 đêm.");
        if (guests < 1 || guests > capacity) throw new BusinessException($"Phòng phục vụ tối đa {capacity} khách.");
    }
    private async Task<Room> AvailableRoom(StayInput input, int? except = null)
    {
        var room = await db.Rooms.Include(x => x.RoomType).SingleOrDefaultAsync(x => x.Id == input.RoomId)
            ?? throw new BusinessException("Không tìm thấy phòng.");
        ValidateStay(input.ArrivalDate, input.DepartureDate, input.GuestCount, room.RoomType.Capacity);
        if (room.OperationalStatus != OperationalStatus.Active) throw new BusinessException("Phòng đang ngừng sử dụng. Vui lòng chọn phòng khác.");
        if (await db.Reservations.AnyAsync(x => x.RoomId == room.Id && x.Id != except &&
            (x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.CheckedIn) &&
            input.ArrivalDate < x.DepartureDate && input.DepartureDate > x.ArrivalDate))
            throw new BusinessException("Phòng vừa được xác nhận cho kỳ ở trùng lịch. Vui lòng chọn phòng hoặc ngày khác.");
        return room;
    }
    public async Task<PageResult<Room>> Search(SearchInput input)
    {
        input.ArrivalDate ??= clock.Today;
        input.DepartureDate ??= input.ArrivalDate.Value.AddDays(2);
        ValidateStay(input.ArrivalDate.Value, input.DepartureDate.Value, input.GuestCount, 20);
        if (input.MinPrice < 0 || input.MaxPrice < 0 || input.MinPrice > input.MaxPrice)
            throw new BusinessException("Khoảng giá chưa hợp lệ.");
        var query = db.Rooms.AsNoTracking().Include(x => x.RoomType)
            .Where(x => x.OperationalStatus == OperationalStatus.Active && x.RoomType.Capacity >= input.GuestCount)
            .Where(x => !x.Reservations.Any(r => (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.CheckedIn) &&
                input.ArrivalDate < r.DepartureDate && input.DepartureDate > r.ArrivalDate));
        if (input.RoomTypeId.HasValue) query = query.Where(x => x.RoomTypeId == input.RoomTypeId);
        if (input.MinPrice.HasValue) query = query.Where(x => x.NightlyRate >= input.MinPrice);
        if (input.MaxPrice.HasValue) query = query.Where(x => x.NightlyRate <= input.MaxPrice);
        var count = await query.CountAsync();
        query = input.Sort switch { "price-desc" => query.OrderByDescending(x => x.NightlyRate).ThenBy(x => x.Id),
            "capacity" => query.OrderByDescending(x => x.RoomType.Capacity).ThenBy(x => x.Id),
            _ => query.OrderBy(x => x.NightlyRate).ThenBy(x => x.Id) };
        input.Page = Math.Clamp(input.Page, 1, Math.Max(1, (count + 8) / 9));
        return new(await query.Skip((input.Page - 1) * 9).Take(9).ToListAsync(), count, input.Page);
    }
    public Task<int> Create(BookingInput input, string actor, string? owner = null) => Atomic(async () =>
    {
        var guest = await db.GuestProfiles.FindAsync(input.GuestProfileId) ?? throw new BusinessException("Không tìm thấy hồ sơ khách.");
        if (owner != null && guest.UserId != owner) throw new BusinessException("Bạn không có quyền sử dụng hồ sơ này.");
        var room = await AvailableRoom(input);
        var reservation = new Reservation { RoomId = room.Id, GuestProfileId = guest.Id,
            ConfirmationCode = "AU-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            ArrivalDate = input.ArrivalDate, DepartureDate = input.DepartureDate, GuestCount = input.GuestCount,
            NightlyRateSnapshot = room.NightlyRate, Notes = input.Notes?.Trim() ?? "", CreatedAt = clock.Now,
            CreatedByUserId = actor, Source = owner == null ? ReservationSource.FrontDesk : ReservationSource.Website,
            Status = ReservationStatus.Pending };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        return reservation.Id;
    });
    public Task Update(int id, StayInput input) => Atomic(async () =>
    {
        var reservation = await GetReservation(id);
        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
            throw new BusinessException("Chỉ sửa đơn chờ xác nhận hoặc đã xác nhận, trước khi nhận phòng.");
        var room = await AvailableRoom(input, id);
        var changed = reservation.RoomId != input.RoomId || reservation.ArrivalDate != input.ArrivalDate || reservation.DepartureDate != input.DepartureDate;
        if (changed || reservation.Status == ReservationStatus.Pending) reservation.NightlyRateSnapshot = room.NightlyRate;
        reservation.RoomId = input.RoomId; reservation.ArrivalDate = input.ArrivalDate;
        reservation.DepartureDate = input.DepartureDate; reservation.GuestCount = input.GuestCount; reservation.Notes = input.Notes ?? "";
    });
    public Task Confirm(int id) => Atomic(async () =>
    {
        var reservation = await GetReservation(id);
        if (reservation.Status == ReservationStatus.Confirmed) return;
        if (reservation.Status != ReservationStatus.Pending) throw new BusinessException("Chỉ xác nhận đơn đang chờ.");
        var room = await AvailableRoom(new StayInput { RoomId = reservation.RoomId, ArrivalDate = reservation.ArrivalDate,
            DepartureDate = reservation.DepartureDate, GuestCount = reservation.GuestCount }, id);
        reservation.NightlyRateSnapshot = room.NightlyRate;
        reservation.Status = ReservationStatus.Confirmed;
    });
    public Task Cancel(int id, string? owner = null, bool reject = false) => Atomic(async () =>
    {
        var reservation = await GetReservation(id);
        if (owner != null && reservation.GuestProfile.UserId != owner) throw new BusinessException("Không tìm thấy đơn đặt phòng của bạn.");
        if (reject && reservation.Status != ReservationStatus.Pending) throw new BusinessException("Chỉ từ chối đơn đang chờ.");
        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed)) throw new BusinessException("Trạng thái này không cho phép hủy.");
        if (owner != null && reservation.ArrivalDate <= clock.Today) throw new BusinessException("Khách chỉ được hủy trước ngày nhận phòng. Vui lòng liên hệ lễ tân.");
        reservation.Status = reject ? ReservationStatus.Rejected : ReservationStatus.Cancelled;
    });
    public Task<int> CheckIn(int id) => Atomic(async () =>
    {
        var reservation = await GetReservation(id);
        if (reservation.Status == ReservationStatus.CheckedIn)
            return (await db.Folios.SingleAsync(x => x.ReservationId == id)).Id;
        if (reservation.Status != ReservationStatus.Confirmed) throw new BusinessException("Đơn phải được xác nhận trước khi nhận phòng.");
        if (reservation.ArrivalDate != clock.Today) throw new BusinessException("Chỉ nhận phòng đúng ngày đã chốt. Hãy điều chỉnh lịch hợp lệ trước.");
        var room = reservation.Room;
        if (room.OperationalStatus != OperationalStatus.Active || room.HousekeepingStatus != HousekeepingStatus.Clean)
            throw new BusinessException("Phòng cần đang hoạt động và đã dọn sạch để nhận khách.");
        if (await db.Reservations.AnyAsync(x => x.RoomId == room.Id && x.Status == ReservationStatus.CheckedIn))
            throw new BusinessException("Phòng vẫn còn khách đang lưu trú.");
        reservation.Status = ReservationStatus.CheckedIn; reservation.CheckInAt = clock.Now;
        var folio = new Folio { ReservationId = id, CreatedAt = clock.Now,
            Items = [new FolioItem { Kind = ItemKind.Room, DescriptionSnapshot = $"Phòng {room.RoomNumber} · {Ui.Date(reservation.ArrivalDate)} – {Ui.Date(reservation.DepartureDate)}",
                Quantity = reservation.Nights, UnitPriceSnapshot = reservation.NightlyRateSnapshot }] };
        db.Folios.Add(folio);
        await db.SaveChangesAsync();
        return folio.Id;
    });
    private async Task<Reservation> GetReservation(int id) => await db.Reservations.Include(x => x.Room).Include(x => x.GuestProfile)
        .SingleOrDefaultAsync(x => x.Id == id) ?? throw new BusinessException("Không tìm thấy đơn đặt phòng.");
    public async Task<Folio> ReadFolio(int id) => await db.Folios.AsNoTracking().Include(x => x.Items).Include(x => x.Payment!).ThenInclude(x => x.ReceivedByUser)
        .Include(x => x.Reservation).ThenInclude(x => x.GuestProfile).Include(x => x.Reservation).ThenInclude(x => x.Room)
        .SingleOrDefaultAsync(x => x.Id == id) ?? throw new BusinessException("Không tìm thấy bảng kê.");
    private async Task<Folio> EditFolio(int id) => await db.Folios.Include(x => x.Items).Include(x => x.Payment)
        .Include(x => x.Reservation).ThenInclude(x => x.Room).SingleOrDefaultAsync(x => x.Id == id)
        ?? throw new BusinessException("Không tìm thấy bảng kê.");
    private static void EnsureEditable(Folio folio)
    {
        if (folio.Status != FolioStatus.Open || folio.Payment != null || folio.Reservation.Status != ReservationStatus.CheckedIn)
            throw new BusinessException("Chỉ sửa dịch vụ khi khách đang lưu trú và chưa thanh toán.");
    }
    public Task Charge(int folioId, ChargeInput input) => Atomic(async () =>
    {
        var folio = await EditFolio(folioId); EnsureEditable(folio);
        if (input.Quantity is < 1 or > 1000) throw new BusinessException("Số lượng phải từ 1 đến 1.000.");
        var service = await db.Services.SingleOrDefaultAsync(x => x.Id == input.ServiceId && x.IsActive)
            ?? throw new BusinessException("Dịch vụ không còn cung cấp.");
        folio.Items.Add(new FolioItem { Kind = ItemKind.Service, ServiceId = service.Id,
            DescriptionSnapshot = service.Name + " / " + service.Unit, Quantity = input.Quantity, UnitPriceSnapshot = service.UnitPrice });
    });
    public Task UpdateCharge(int folioId, int itemId, int quantity, bool delete = false) => Atomic(async () =>
    {
        var folio = await EditFolio(folioId); EnsureEditable(folio);
        var item = folio.Items.SingleOrDefault(x => x.Id == itemId && x.Kind == ItemKind.Service)
            ?? throw new BusinessException("Không tìm thấy khoản dịch vụ.");
        if (delete) db.FolioItems.Remove(item);
        else {
            if (quantity is < 1 or > 1000) throw new BusinessException("Số lượng phải từ 1 đến 1.000.");
            item.Quantity = quantity;
        }
    });
    private static void ValidateRoomCharge(Folio folio)
    {
        var roomLines = folio.Items.Where(x => x.Kind == ItemKind.Room).ToList();
        if (roomLines.Count != 1 || roomLines[0].Quantity != folio.Reservation.Nights ||
            roomLines[0].UnitPriceSnapshot != folio.Reservation.NightlyRateSnapshot || folio.Total <= 0)
            throw new BusinessException("Bảng kê chưa có đúng phí phòng. Không thể thanh toán hoặc trả phòng.");
    }
    public Task Pay(int folioId, PaymentInput input, string actor) => Atomic(async () =>
    {
        if (!Enum.IsDefined(input.Method)) throw new BusinessException("Phương thức thanh toán không hợp lệ.");
        var folio = await EditFolio(folioId);
        if (folio.Payment != null) {
            if (folio.Payment.Amount == input.Amount && folio.Payment.Method == input.Method) return;
            throw new BusinessException("Bảng kê đã được thanh toán. Không thể thu thêm hoặc thay đổi thanh toán.");
        }
        EnsureEditable(folio); ValidateRoomCharge(folio);
        if (input.Amount <= 0 || input.Amount != folio.Total) throw new BusinessException("Số tiền nhận phải bằng chính xác tổng phải thu.");
        folio.Payment = new Payment { Amount = folio.Total, Method = input.Method, PaidAt = clock.Now, ReceivedByUserId = actor };
        folio.Status = FolioStatus.Paid;
    });
    public Task CheckOut(int folioId) => Atomic(async () =>
    {
        var folio = await EditFolio(folioId);
        if (folio.Status == FolioStatus.Closed && folio.Reservation.Status == ReservationStatus.CheckedOut) return;
        ValidateRoomCharge(folio);
        if (folio.Reservation.Status != ReservationStatus.CheckedIn || folio.Status != FolioStatus.Paid || folio.Payment == null || folio.Balance != 0)
            throw new BusinessException("Chỉ trả phòng khi khách đang ở và đã thanh toán đủ.");
        folio.Reservation.Status = ReservationStatus.CheckedOut; folio.Reservation.CheckOutAt = clock.Now;
        folio.Status = FolioStatus.Closed; folio.ClosedAt = clock.Now;
        folio.Reservation.Room.HousekeepingStatus = HousekeepingStatus.Dirty;
    });
    public Task SetHousekeeping(int roomId, HousekeepingStatus value) => Atomic(async () =>
    {
        if (!Enum.IsDefined(value)) throw new BusinessException("Tình trạng dọn phòng không hợp lệ.");
        var room = await db.Rooms.FindAsync(roomId) ?? throw new BusinessException("Không tìm thấy phòng.");
        room.HousekeepingStatus = value;
    });
    public Task SetOperation(int roomId, OperationalStatus value) => Atomic(async () =>
    {
        if (!Enum.IsDefined(value)) throw new BusinessException("Tình trạng khai thác không hợp lệ.");
        var room = await db.Rooms.FindAsync(roomId) ?? throw new BusinessException("Không tìm thấy phòng.");
        if (value == OperationalStatus.OutOfOrder && await db.Reservations.AnyAsync(x => x.RoomId == roomId &&
            (x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.CheckedIn)))
            throw new BusinessException("Phòng còn đơn đã xác nhận hoặc có khách. Hãy xử lý các đơn trước khi ngừng sử dụng.");
        room.OperationalStatus = value;
    });
}
