using EasterEggHunt.Application;
using EasterEggHunt.Infrastructure;
using EasterEggHunt.Infrastructure.Configuration;
using EasterEggHunt.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Easter Egg Hunt Configuration
builder.Services.AddEasterEggHuntConfiguration(builder.Configuration);

// Add CORS
builder.Services.AddEasterEggHuntCors(builder.Configuration);

// Add Easter Egg Hunt DbContext
builder.Services.AddEasterEggHuntDbContext(builder.Configuration);

// Add Repositories
builder.Services.AddRepositories();

// Add Application Services
builder.Services.AddApplicationServices();

// Add Seed Data Service (Development only)
builder.Services.AddSeedDataService(builder.Environment);

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

// Configure EasterEggHunt environment-specific settings
app.ConfigureEasterEggHuntEnvironment();

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
