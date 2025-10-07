using EasterEggHunt.Api.Configuration;
using EasterEggHunt.Application;
using EasterEggHunt.Infrastructure;
using EasterEggHunt.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
    // Hot-Reload is automatically enabled when using 'dotnet watch run'
}

// Configure EasterEggHunt environment-specific settings
app.ConfigureEasterEggHuntEnvironment();

app.UseCors();

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
