using EasterEggHunt.Domain.Configuration;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Api.Configuration;

/// <summary>
/// API-spezifische Konfigurationsextensions
/// </summary>
public static class ApiConfigurationExtensions
{
    /// <summary>
    /// Konfiguriert CORS für die API
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <param name="configuration">Konfiguration</param>
    /// <returns>Service-Collection</returns>
    public static IServiceCollection AddEasterEggHuntCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(EasterEggHuntOptions.SectionName)
            .Get<EasterEggHuntOptions>();

        if (options?.Security.AllowedOrigins?.Count > 0)
        {
            services.AddCors(corsOptions =>
            {
                corsOptions.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(options.Security.AllowedOrigins.ToArray())
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
        else
        {
            // Development-Fallback
            services.AddCors(corsOptions =>
            {
                corsOptions.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }

        return services;
    }

    /// <summary>
    /// Konfiguriert die API-Anwendung basierend auf der Environment
    /// </summary>
    /// <param name="app">Web-Application</param>
    /// <returns>Web-Application</returns>
    public static WebApplication ConfigureEasterEggHuntEnvironment(this WebApplication app)
    {
        var options = app.Configuration
            .GetSection(EasterEggHuntOptions.SectionName)
            .Get<EasterEggHuntOptions>();

        if (options == null)
        {
            return app;
        }

        // Datenbank-Migration konfigurieren
        if (options.Database.AutoMigrate)
        {
            app.ConfigureDatabaseMigration();
        }

        // HTTPS-Redirect konfigurieren
        if (options.Security.RequireHttps)
        {
            app.UseHttpsRedirection();
        }

        // Response-Kompression konfigurieren
        if (options.Performance.EnableResponseCompression)
        {
            app.UseResponseCompression();
        }

        // Response-Caching konfigurieren
        if (options.Performance.EnableResponseCaching)
        {
            app.UseResponseCaching();
        }

        return app;
    }

    /// <summary>
    /// Konfiguriert die Datenbank-Migration
    /// </summary>
    /// <param name="app">Web-Application</param>
    private static void ConfigureDatabaseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            logger.LogInformation("Starte Datenbank-Migration...");
            context.Database.Migrate();
            logger.LogInformation("Datenbank-Migration erfolgreich abgeschlossen.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler bei der Datenbank-Migration");
            throw;
        }
    }
}
