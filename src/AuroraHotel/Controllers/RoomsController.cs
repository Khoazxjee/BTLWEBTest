using AuroraHotel.Data;
using AuroraHotel.Services;
using AuroraHotel.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AuroraHotel.Controllers;
public class RoomsController(HotelDbContext db, HotelOperations operations, IHotelClock clock) : Controller
{
    public async Task<IActionResult> Index(SearchInput input)
    {
        PageResult<Models.Room> result = new([], 0, 1);
        try { if (ModelState.IsValid) result = await operations.Search(input); else ModelState.AddModelError("", "Vui lòng kiểm tra các tiêu chí tìm kiếm."); }
        catch (BusinessException e) { ModelState.AddModelError("", e.Message); }
        var page = new SearchPage(input, result, await db.RoomTypes.AsNoTracking().ToListAsync());
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ? PartialView("_Results", page) : View(page);
    }
    public async Task<IActionResult> Details(int id, DateOnly? arrivalDate, DateOnly? departureDate, int guestCount = 2, string? returnUrl = null)
    {
        var room = await db.Rooms.AsNoTracking().Include(x => x.RoomType).SingleOrDefaultAsync(x => x.Id == id && x.OperationalStatus == Models.OperationalStatus.Active);
        if (room == null) return NotFound();
        ViewBag.SearchReturnUrl = returnUrl != null && Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/Rooms", StringComparison.OrdinalIgnoreCase) ? returnUrl : null;
        ViewBag.Stay = new StayInput { RoomId = id, ArrivalDate = arrivalDate ?? clock.Today, DepartureDate = departureDate ?? (arrivalDate ?? clock.Today).AddDays(2), GuestCount = guestCount };
        return View(room);
    }
}
