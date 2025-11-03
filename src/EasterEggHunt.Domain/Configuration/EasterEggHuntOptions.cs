namespace EasterEggHunt.Domain.Configuration;

/// <summary>
/// Konfigurationsoptionen für das Easter Egg Hunt System
/// </summary>
public class EasterEggHuntOptions
{
    public const string SectionName = "EasterEggHunt";

    /// <summary>
    /// Datenbank-Konfiguration
    /// </summary>
    public DatabaseOptions Database { get; set; } = new();

    /// <summary>
    /// Sicherheits-Konfiguration
    /// </summary>
    public SecurityOptions Security { get; set; } = new();

    /// <summary>
    /// Feature-Flags
    /// </summary>
    public FeatureOptions Features { get; set; } = new();

    /// <summary>
    /// Performance-Konfiguration
    /// </summary>
    public PerformanceOptions Performance { get; set; } = new();

    /// <summary>
    /// UI-Konfiguration (nur für Web-Projekt)
    /// </summary>
    public UIOptions? UI { get; set; }

    /// <summary>
    /// Session-Konfiguration
    /// </summary>
    public SessionOptions Session { get; set; } = new();
}

/// <summary>
/// Datenbank-Konfiguration
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Datenbank-Provider (SQLite, SQLServer, PostgreSQL)
    /// </summary>
    public string Provider { get; set; } = "SQLite";

    /// <summary>
    /// Verbindungszeichenfolge
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Automatische Migration beim Start aktivieren
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Seed-Daten beim Start laden
    /// </summary>
    public bool SeedData { get; set; } = true;
}

/// <summary>
/// Sicherheits-Konfiguration
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// HTTPS erforderlich
    /// </summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// Erlaubte Ursprünge für CORS
    /// </summary>
    public IReadOnlyList<string> AllowedOrigins { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Feature-Flags
/// </summary>
public class FeatureOptions
{
    /// <summary>
    /// Swagger UI aktivieren
    /// </summary>
    public bool EnableSwagger { get; set; } = true;

    /// <summary>
    /// Detaillierte Fehlermeldungen aktivieren
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = true;

    /// <summary>
    /// Sensible Daten in Logs aktivieren
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

/// <summary>
/// Performance-Konfiguration
/// </summary>
public class PerformanceOptions
{
    /// <summary>
    /// Response-Kompression aktivieren
    /// </summary>
    public bool EnableResponseCompression { get; set; } = false;

    /// <summary>
    /// Response-Caching aktivieren
    /// </summary>
    public bool EnableResponseCaching { get; set; } = false;

    /// <summary>
    /// Cache-Ablaufzeit in Minuten
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;
}

/// <summary>
/// UI-Konfiguration
/// </summary>
public class UIOptions
{
    /// <summary>
    /// Anwendungs-Titel
    /// </summary>
    public string Title { get; set; } = "Easter Egg Hunt System";

    /// <summary>
    /// Firmenname
    /// </summary>
    public string CompanyName { get; set; } = "Your Company";

    /// <summary>
    /// Support-E-Mail
    /// </summary>
    public string SupportEmail { get; set; } = "support@example.com";
}

/// <summary>
/// Session-Konfiguration
/// </summary>
public class SessionOptions
{
    /// <summary>
    /// Session-Bereinigung aktivieren
    /// </summary>
    public bool CleanupEnabled { get; set; } = true;

    /// <summary>
    /// Interval für Session-Bereinigung in Stunden (Standard: 24 Stunden)
    /// </summary>
    public int CleanupIntervalHours { get; set; } = 24;

    /// <summary>
    /// Initiales Delay für Session-Bereinigung in Sekunden (Standard: 30 Sekunden)
    /// </summary>
    public int CleanupInitialDelaySeconds { get; set; } = 30;
}
