using EasterEggHunt.Web.Configuration;
using EasterEggHunt.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
