using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Configuration;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Infrastructure;

/// <summary>
/// Extension-Methoden für Dependency Injection-Konfiguration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registriert den EasterEggHuntDbContext und verwandte Services
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <param name="configuration">Konfiguration</param>
    /// <returns>Service-Collection für Method-Chaining</returns>
    public static IServiceCollection AddEasterEggHuntDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext registrieren mit zentraler Konfiguration
        services.AddDbContext<EasterEggHuntDbContext>(options =>
        {
            DbContextConfiguration.ConfigureDbContext(options, configuration);
        });

        // DbContext als Scoped Service registrieren
        services.AddScoped<EasterEggHuntDbContext>();

        return services;
    }

    /// <summary>
    /// Registriert den EasterEggHuntDbContext mit benutzerdefinierten Optionen
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <param name="configureOptions">Konfigurations-Action</param>
    /// <returns>Service-Collection für Method-Chaining</returns>
    public static IServiceCollection AddEasterEggHuntDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        // DbContext mit benutzerdefinierten Optionen registrieren
        services.AddDbContext<EasterEggHuntDbContext>(configureOptions);

        // DbContext als Scoped Service registrieren
        services.AddScoped<EasterEggHuntDbContext>();

        return services;
    }

    /// <summary>
    /// Registriert alle Repository Services
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <returns>Service-Collection für Method-Chaining</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Repository Interfaces mit Implementierungen registrieren
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IQrCodeRepository, QrCodeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFindRepository, FindRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();

        return services;
    }

    /// <summary>
    /// Registriert den SeedDataService für Entwicklungsdaten
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <param name="environment">Hosting Environment</param>
    /// <returns>Service-Collection für Method-Chaining</returns>
    public static IServiceCollection AddSeedDataService(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        // SeedDataService nur in Development registrieren
        if (string.Equals(environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHostedService<SeedDataService>();
        }

        return services;
    }

    /// <summary>
    /// Registriert den SessionCleanupService für automatische Session-Bereinigung
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <param name="configuration">Konfiguration</param>
    /// <returns>Service-Collection für Method-Chaining</returns>
    public static IServiceCollection AddSessionCleanupService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Konfiguration aus EasterEggHuntOptions lesen
        var options = configuration
            .GetSection(EasterEggHunt.Domain.Configuration.EasterEggHuntOptions.SectionName)
            .Get<EasterEggHunt.Domain.Configuration.EasterEggHuntOptions>();

        var sessionOptions = options?.Session ?? new EasterEggHunt.Domain.Configuration.SessionOptions();
        var cleanupInterval = TimeSpan.FromHours(sessionOptions.CleanupIntervalHours);
        var initialDelay = TimeSpan.FromSeconds(sessionOptions.CleanupInitialDelaySeconds);

        // SessionCleanupService registrieren
        services.AddHostedService(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<SessionCleanupService>>();
            return new SessionCleanupService(
                serviceProvider,
                logger,
                cleanupInterval,
                initialDelay,
                sessionOptions.CleanupEnabled);
        });

        return services;
    }
}
