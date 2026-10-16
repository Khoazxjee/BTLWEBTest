using System.Globalization;
using AuroraHotel.Data;
using AuroraHotel.Models;
using AuroraHotel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var initializeDemo = args.Contains("--initialize-demo", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(x => x != "--initialize-demo").ToArray());
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
builder.Services.Configure<HotelOptions>(builder.Configuration.GetSection("Hotel"));
builder.Services.AddDbContext<HotelDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("HotelDb")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:HotelDb before starting.")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o => {
    o.User.RequireUniqueEmail = true; o.Password.RequiredLength = 8;
    o.Lockout.MaxFailedAccessAttempts = 5; o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
}).AddEntityFrameworkStores<HotelDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(o => {
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied";
    o.Cookie.Name = "Aurora.Auth"; o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.Events.OnRedirectToLogin = c => { if (c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode = 401; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = c => { if (c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode = 403; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
});
builder.Services.AddControllersWithViews(o => {
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập giá trị.");
    o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, name) => $"Giá trị {name} không hợp lệ.");
});
builder.Services.AddAntiforgery(o => {
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => {
    o.Cookie.Name = "Aurora.Selection"; o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddHttpsRedirection(o => o.HttpsPort = 443);
builder.Services.AddSingleton<IHotelClock, HotelClock>();
builder.Services.AddScoped<HotelOperations>(); builder.Services.AddScoped<CatalogService>(); builder.Services.AddScoped<SeedData>();
var app = builder.Build();
if (initializeDemo) {
    await using var scope = app.Services.CreateAsyncScope();
    var credentials = await scope.ServiceProvider.GetRequiredService<SeedData>().InitializeHostedDemo(
        app.Configuration["Bootstrap:DatabaseName"] ?? "", app.Configuration["Bootstrap:DemoPassword"] ?? "");
    var accountFolder = Path.Combine(app.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(accountFolder);
    await File.WriteAllLinesAsync(Path.Combine(accountFolder, "hosting-demo-accounts.txt"),
        new[] { "TÀI KHOẢN DEMO HOSTING — giữ riêng, không đưa lên GitHub. Mỗi tài khoản có mật khẩu riêng." }
        .Concat(credentials.Select(x => $"{x.Key} | {x.Value}")));
    Console.WriteLine("Đã khởi tạo database demo hosting: 4 tài khoản, 12 phòng và các đơn minh họa. Không khởi chạy web server.");
    Console.WriteLine("Thông tin đăng nhập riêng từng vai trò: App_Data/hosting-demo-accounts.txt trên máy chạy lệnh.");
    return;
}
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (context, next) => {
    context.Response.Headers.XContentTypeOptions = "nosniff"; context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self'; font-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("vi-VN"), SupportedCultures = [new CultureInfo("vi-VN")], SupportedUICultures = [new CultureInfo("vi-VN")] });
app.UseStaticFiles(); app.UseRouting(); app.UseSession(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
if (app.Environment.IsDevelopment()) {
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<HotelDbContext>().Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("SeedDemo")) await scope.ServiceProvider.GetRequiredService<SeedData>().Run();
}
app.Run();
public partial class Program;
