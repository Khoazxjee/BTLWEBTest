using AuroraHotel.Data;
using AuroraHotel.Models;
using AuroraHotel.Services;
using AuroraHotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AuroraHotel.Areas.Staff.Controllers;
[Authorize(Roles = Roles.Admin)]
public class DashboardController(HotelDbContext db, IHotelClock clock) : StaffController
{
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to)
    {
        var start = from ?? new DateOnly(clock.Today.Year, clock.Today.Month, 1); var end = to ?? clock.Today;
        if (end < start) { TempData["Error"] = "Ngày kết thúc cần bằng hoặc sau ngày bắt đầu."; end = start; }
        var offset = clock.Now.Offset;
        var a = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), offset); var z = new DateTimeOffset(end.AddDays(1).ToDateTime(TimeOnly.MinValue), offset);
        var payments = db.Payments.Where(x => x.PaidAt >= a && x.PaidAt < z);
        var counts = await db.Reservations.Where(x => x.CreatedAt >= a && x.CreatedAt < z).GroupBy(x => x.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Status, x => x.Count);
        return View(new DashboardModel(start, end, await db.Reservations.CountAsync(x => x.Status == ReservationStatus.CheckedIn),
            await db.Rooms.CountAsync(x => x.HousekeepingStatus == HousekeepingStatus.Dirty), await db.Rooms.CountAsync(x => x.OperationalStatus == OperationalStatus.Active),
            await payments.SumAsync(x => (decimal?)x.Amount) ?? 0, await payments.CountAsync(), counts,
            await db.Reservations.AsNoTracking().Include(x => x.Room).Include(x => x.GuestProfile).OrderByDescending(x => x.CreatedAt).Take(6).ToListAsync()));
    }
}
