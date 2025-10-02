using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using EasterEggHunt.Infrastructure.Configuration;

namespace EasterEggHunt.Infrastructure.Data;

/// <summary>
/// Factory für DbContext-Erstellung während Design-Time (z.B. für Migrations)
/// 
/// Hinweis: Diese Factory lädt die Konfiguration direkt, da sie außerhalb des 
/// Dependency Injection Containers läuft. Für Runtime wird die Konfiguration 
/// über DI in ServiceCollectionExtensions registriert.
/// </summary>
public class EasterEggHuntDbContextFactory : IDesignTimeDbContextFactory<EasterEggHuntDbContext>
{
    /// <summary>
    /// Erstellt eine neue DbContext-Instanz für Design-Time-Operationen
    /// </summary>
    /// <param name="args">Command-Line-Argumente</param>
    /// <returns>Konfigurierte DbContext-Instanz</returns>
    public EasterEggHuntDbContext CreateDbContext(string[] args)
    {
        // Design-Time Konfiguration laden (außerhalb des DI-Containers)
        var configuration = DbContextConfiguration.BuildDesignTimeConfiguration();

        // DbContext-Optionen erstellen und mit zentraler Konfiguration konfigurieren
        var optionsBuilder = new DbContextOptionsBuilder<EasterEggHuntDbContext>();
        DbContextConfiguration.ConfigureDbContext(optionsBuilder, configuration, isDesignTime: true);

        return new EasterEggHuntDbContext(optionsBuilder.Options);
    }
}
