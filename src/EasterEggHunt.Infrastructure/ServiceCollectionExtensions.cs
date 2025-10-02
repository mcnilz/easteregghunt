using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Configuration;

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
}
