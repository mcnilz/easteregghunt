using EasterEggHunt.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasterEggHunt.Application;

/// <summary>
/// Extension Methods für Service Registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registriert alle Application Services
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection für Method Chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Service Interfaces und Implementierungen registrieren
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFindService, FindService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        return services;
    }
}
