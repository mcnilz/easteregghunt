using EasterEggHunt.Web.Configuration;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Authentication with multiple Cookie schemes (Admin and Employee)
builder.Services.AddAuthentication(options =>
{
    // Set default scheme based on request path
    options.DefaultScheme = "DynamicScheme";
})
.AddCookie("AdminScheme", options =>
{
    // Admin Authentication Schema
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "EasterEggHunt.Admin";
    options.Cookie.HttpOnly = true;
    // In Development: Allow HTTP cookies, in Production: Require HTTPS
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddCookie("EmployeeScheme", options =>
{
    // Employee Authentication Schema
    options.LoginPath = "/Employee/Register";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.Name = "EasterEggHunt.Employee";
    options.Cookie.HttpOnly = true;
    // In Development: Allow HTTP cookies, in Production: Require HTTPS
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Lax für bessere Kompatibilität mit QR-Code-Scans
})
.AddPolicyScheme("DynamicScheme", "Dynamic Authentication", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // QR-Code routes verwenden EmployeeScheme
        if (context.Request.Path.StartsWithSegments("/qr", StringComparison.OrdinalIgnoreCase))
        {
            return "EmployeeScheme";
        }

        // Employee routes verwenden EmployeeScheme
        if (context.Request.Path.StartsWithSegments("/Employee", StringComparison.OrdinalIgnoreCase))
        {
            return "EmployeeScheme";
        }

        // Alle anderen routes verwenden AdminScheme
        return "AdminScheme";
    };
});

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // In Development: Allow HTTP cookies, in Production: Require HTTPS
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add HTTP Client for API communication
builder.Services.AddHttpClient<IEasterEggHuntApiClient, EasterEggHuntApiClient>(client =>
{
    // Configure API base URL from configuration
    var apiBaseUrl = builder.Configuration["EasterEggHunt:Api:BaseUrl"] ?? "https://localhost:7002";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Hot-Reload is automatically enabled when using 'dotnet watch run'
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "qrcode",
    pattern: "qr/{code}",
    defaults: new { controller = "Employee", action = "ScanQrCode" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
