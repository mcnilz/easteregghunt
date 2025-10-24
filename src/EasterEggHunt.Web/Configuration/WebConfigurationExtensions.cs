using EasterEggHunt.Domain.Configuration;

namespace EasterEggHunt.Web.Configuration;

/// <summary>
/// Web-spezifische Konfigurationsextensions
/// </summary>
public static class WebConfigurationExtensions
{
    /// <summary>
    /// Konfiguriert CORS für das Web-Projekt
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
    /// Konfiguriert die Web-Anwendung basierend auf der Environment
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

        // Datenbank-Migration wird über die API gehandhabt
        // Das Web-Projekt hat keine direkte Datenbank-Verbindung mehr

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
}
