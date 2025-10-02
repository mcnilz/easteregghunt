using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EasterEggHunt.Infrastructure.Configuration;

/// <summary>
/// Zentrale Konfiguration für DbContext-Optionen
/// </summary>
public static class DbContextConfiguration
{
    /// <summary>
    /// Konfiguriert DbContext-Optionen für Runtime (über DI)
    /// </summary>
    /// <param name="optionsBuilder">DbContext-Options-Builder</param>
    /// <param name="configuration">Konfiguration aus DI</param>
    public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        // Connection String aus Konfiguration laden
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=easteregghunt.db";

        optionsBuilder.UseSqlite(connectionString);

        // Development-spezifische Konfiguration
        if (IsDevelopmentEnvironment())
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }

    /// <summary>
    /// Konfiguriert DbContext-Optionen für Design-Time (ohne DI)
    /// </summary>
    /// <param name="optionsBuilder">DbContext-Options-Builder</param>
    /// <param name="configuration">Design-Time Konfiguration</param>
    public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration, bool isDesignTime)
    {
        // Connection String aus Konfiguration laden
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=easteregghunt.db";

        optionsBuilder.UseSqlite(connectionString);

        // Development-spezifische Konfiguration
        if (IsDevelopmentEnvironment())
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }

    /// <summary>
    /// Lädt die Konfiguration für Design-Time-Operationen
    /// </summary>
    /// <returns>Konfigurationsinstanz</returns>
    public static IConfiguration BuildDesignTimeConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Prüft, ob die Anwendung im Development-Modus läuft
    /// </summary>
    /// <returns>True wenn Development-Modus aktiv</returns>
    private static bool IsDevelopmentEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
}
