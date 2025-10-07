using EasterEggHunt.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasterEggHunt.Infrastructure.Configuration;

/// <summary>
/// Extension-Methoden für EasterEggHunt-Konfiguration
/// </summary>
public static class EasterEggHuntConfigurationExtensions
{
    /// <summary>
    /// Registriert EasterEggHunt-Konfigurationsoptionen
    /// </summary>
    /// <param name="services">Service-Collection</param>
    /// <param name="configuration">Konfiguration</param>
    /// <returns>Service-Collection</returns>
    public static IServiceCollection AddEasterEggHuntConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EasterEggHuntOptions>(
            options => configuration.GetSection(EasterEggHuntOptions.SectionName).Bind(options));

        return services;
    }


}
