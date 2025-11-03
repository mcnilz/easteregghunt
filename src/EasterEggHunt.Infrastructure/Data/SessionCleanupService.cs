using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Infrastructure.Data;

/// <summary>
/// Background Service zur automatischen Bereinigung abgelaufener Sessions
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly TimeSpan _initialDelay;
    private readonly bool _isEnabled;

    /// <summary>
    /// Konstruktor für Dependency Injection
    /// </summary>
    /// <param name="serviceProvider">Service Provider für SessionRepository</param>
    /// <param name="logger">Logger für Cleanup-Operationen</param>
    /// <param name="cleanupInterval">Interval zwischen Bereinigungsläufen (Standard: 24 Stunden)</param>
    /// <param name="initialDelay">Initiales Delay vor dem ersten Bereinigungslauf (Standard: 30 Sekunden)</param>
    /// <param name="isEnabled">Gibt an, ob der Cleanup-Service aktiviert ist</param>
    public SessionCleanupService(
        IServiceProvider serviceProvider,
        ILogger<SessionCleanupService> logger,
        TimeSpan cleanupInterval,
        TimeSpan initialDelay,
        bool isEnabled = true)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cleanupInterval = cleanupInterval;
        _initialDelay = initialDelay;
        _isEnabled = isEnabled;
    }

    /// <summary>
    /// Haupt-Loop für die Session-Bereinigung
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Session-Cleanup-Service ist deaktiviert.");
            return;
        }

        _logger.LogInformation(
            "Session-Cleanup-Service gestartet. Bereinigungs-Interval: {Interval}, Initiales Delay: {InitialDelay}",
            _cleanupInterval,
            _initialDelay);

        // Initiale Wartezeit, damit die Anwendung vollständig gestartet ist
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredSessionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service wird gestoppt, normal
                _logger.LogInformation("Session-Cleanup-Service wird gestoppt");
                break;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            // Background Services müssen robust sein und auch bei unerwarteten Exceptions weiterlaufen
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Fehler bei der Session-Bereinigung (InvalidOperation). Nächster Versuch in {Interval}", _cleanupInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unerwarteter Fehler bei der Session-Bereinigung. Nächster Versuch in {Interval}", _cleanupInterval);
            }
#pragma warning restore CA1031

            // Warten auf nächstes Interval
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Session-Cleanup-Service wurde gestoppt.");
    }

    /// <summary>
    /// Bereinigt abgelaufene Sessions
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    private async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetService<ISessionRepository>();
        if (sessionRepository == null)
        {
            _logger.LogError("ISessionRepository service nicht gefunden. Bereinigung übersprungen.");
            throw new InvalidOperationException("ISSessionRepository service not found");
        }

        try
        {
            _logger.LogDebug("Starte Bereinigung abgelaufener Sessions...");

            var deletedCount = await sessionRepository.DeleteExpiredAsync();

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Bereinigung abgeschlossen. {DeletedCount} abgelaufene Session(s) gelöscht.",
                    deletedCount);
            }
            else
            {
                _logger.LogDebug("Keine abgelaufenen Sessions gefunden.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Bereinigen abgelaufener Sessions");
            throw;
        }
    }
}

