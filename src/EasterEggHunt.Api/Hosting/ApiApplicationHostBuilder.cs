using EasterEggHunt.Api.Configuration;
using EasterEggHunt.Api.Middleware;
using EasterEggHunt.Application;
using EasterEggHunt.Infrastructure;
using EasterEggHunt.Infrastructure.Configuration;

namespace EasterEggHunt.Api.Hosting;

/// <summary>
/// Builder-Klasse für die Konfiguration der API-Anwendung
/// </summary>
public static class ApiApplicationHostBuilder
{
    /// <summary>
    /// Konfiguriert die Services für die API-Anwendung
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // XML-Kommentare einbinden
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Swagger-Dokumentation konfigurieren
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Version = "v1",
                Title = "Easter Egg Hunt API",
                Description = "RESTful API für das Easter Egg Hunt System. " +
                             "Ermöglicht Mitarbeitern QR-Codes zu scannen, Funde zu registrieren und Statistiken abzurufen.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Easter Egg Hunt Team"
                }
            });
        });

        // Add Easter Egg Hunt Configuration
        builder.Services.AddEasterEggHuntConfiguration(builder.Configuration);

        // Add CORS
        builder.Services.AddEasterEggHuntCors(builder.Configuration);

        // Add Response Compression and Response Caching (if enabled in configuration)
        var options = builder.Configuration
            .GetSection(EasterEggHunt.Domain.Configuration.EasterEggHuntOptions.SectionName)
            .Get<EasterEggHunt.Domain.Configuration.EasterEggHuntOptions>();
        if (options?.Performance.EnableResponseCompression == true)
        {
            builder.Services.AddResponseCompression();
        }

        if (options?.Performance.EnableResponseCaching == true)
        {
            builder.Services.AddResponseCaching();
        }

        // Add Easter Egg Hunt DbContext
        builder.Services.AddEasterEggHuntDbContext(builder.Configuration);

        // Add Repositories
        builder.Services.AddRepositories();

        // Add Application Services
        builder.Services.AddApplicationServices();

        // Add Seed Data Service (Development only)
        builder.Services.AddSeedDataService(builder.Environment);

        // Add Session Cleanup Service
        builder.Services.AddSessionCleanupService(builder.Configuration);
    }

    /// <summary>
    /// Konfiguriert die HTTP Request Pipeline für die API-Anwendung
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void ConfigurePipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            // Hot-Reload is automatically enabled when using 'dotnet watch run'
        }

        // Configure EasterEggHunt environment-specific settings
        app.ConfigureEasterEggHuntEnvironment();

        // Performance-Tracking Middleware
        app.UseMiddleware<PerformanceMiddleware>();

        app.UseCors();

        app.UseAuthorization();

        // Map controllers
        app.MapControllers();

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithOpenApi();
    }
}


