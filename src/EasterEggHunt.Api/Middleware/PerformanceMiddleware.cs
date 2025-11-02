using System.Diagnostics;

namespace EasterEggHunt.Api.Middleware;

/// <summary>
/// Middleware zur Messung der Response-Zeit
/// </summary>
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;
    private const int SlowRequestThresholdMs = 1000; // 1 Sekunde

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Performance-Metriken in Context.Items speichern
            context.Items["RequestDurationMs"] = elapsedMs;
            context.Items["RequestPath"] = path.ToString();

            // Langsame Requests loggen
            if (elapsedMs > SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "Langsame Anfrage erkannt: {Path} dauerte {DurationMs}ms",
                    path,
                    elapsedMs);
            }
            else
            {
                _logger.LogDebug(
                    "Anfrage abgeschlossen: {Path} dauerte {DurationMs}ms",
                    path,
                    elapsedMs);
            }
        }
    }
}

