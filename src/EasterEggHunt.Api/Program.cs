using EasterEggHunt.Infrastructure;
using EasterEggHunt.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
