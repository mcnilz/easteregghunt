using EasterEggHunt.Domain.Configuration;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Api.Configuration;

/// <summary>
/// Gemeinsame Konfigurationserweiterungen für EasterEggHunt-Anwendungen
/// </summary>
internal static class CommonConfigurationExtensions
{
    /// <summary>
    /// Konfiguriert die Datenbank-Migration
    /// </summary>
    /// <typeparam name="TContext">DbContext-Typ</typeparam>
    /// <param name="services">Service Provider</param>
    /// <param name="options">EasterEggHunt-Optionen</param>
    public static void ConfigureDatabaseMigration<TContext>(this IServiceProvider services, EasterEggHuntOptions options)
        where TContext : DbContext
    {
        if (!options.Database.AutoMigrate)
        {
            return;
        }

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceProvider>>();

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
