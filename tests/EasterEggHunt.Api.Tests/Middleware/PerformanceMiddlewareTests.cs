using System.Diagnostics;
using EasterEggHunt.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Api.Tests.Middleware;

/// <summary>
/// Unit Tests für PerformanceMiddleware
/// </summary>
[TestFixture]
public class PerformanceMiddlewareTests
{
    private Mock<RequestDelegate> _mockNext = null!;
    private Mock<ILogger<PerformanceMiddleware>> _mockLogger = null!;
    private PerformanceMiddleware _middleware = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void Setup()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<PerformanceMiddleware>>();
        _middleware = new PerformanceMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/test";
    }

    #region InvokeAsync Tests

    /// <summary>
    /// Testet, dass die Middleware die Request-Zeit misst und in Context.Items speichert.
    /// Wichtig, da diese Middleware für Performance-Monitoring verwendet wird.
    /// Stellt sicher, dass Response-Zeiten korrekt gemessen werden können.
    /// </summary>
    [Test]
    public async Task InvokeAsync_ShouldMeasureResponseTime()
    {
        // Arrange
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(_httpContext.Items.ContainsKey("RequestDurationMs"), Is.True);
        Assert.That(_httpContext.Items["RequestDurationMs"], Is.InstanceOf<long>());
        var duration = (long)_httpContext.Items["RequestDurationMs"]!;
        Assert.That(duration, Is.GreaterThanOrEqualTo(0));
        Assert.That(_httpContext.Items["RequestPath"], Is.EqualTo("/api/test"));
    }

    /// <summary>
    /// Testet, dass langsame Requests (> 1s) als Warning geloggt werden.
    /// Wichtig, um Performance-Probleme frühzeitig zu erkennen.
    /// Verhindert, dass langsame Requests unbemerkt bleiben.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithSlowRequest_ShouldLogWarning()
    {
        // Arrange
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(async (HttpContext context) =>
            {
                await Task.Delay(1100); // > 1s
            });

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Langsame Anfrage")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testet, dass normale Requests als Debug geloggt werden.
    /// Wichtig, um Log-Spam zu vermeiden, während wichtige Informationen
    /// für Debugging-Zwecke verfügbar bleiben.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithNormalRequest_ShouldLogDebug()
    {
        // Arrange
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Anfrage abgeschlossen")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Testet, dass die Middleware auch bei Exceptions die Performance misst.
    /// Wichtig, um sicherzustellen, dass Performance-Tracking auch bei Fehlern funktioniert.
    /// Verhindert, dass Performance-Daten bei Exceptions verloren gehen.
    /// </summary>
    [Test]
    public void InvokeAsync_WhenExceptionOccurs_ShouldStillMeasurePerformance()
    {
        // Arrange
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _middleware.InvokeAsync(_httpContext));
        Assert.That(ex, Is.Not.Null);

        // Assert - Performance sollte trotzdem gemessen werden
        Assert.That(_httpContext.Items.ContainsKey("RequestDurationMs"), Is.True);
        Assert.That(_httpContext.Items["RequestDurationMs"], Is.InstanceOf<long>());
    }

    /// <summary>
    /// Testet, dass die Middleware verschiedene Pfade korrekt verarbeitet.
    /// Wichtig, um sicherzustellen, dass Performance-Tracking für alle Endpoints funktioniert.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithDifferentPaths_ShouldStoreCorrectPath()
    {
        // Arrange
        var testPaths = new[] { "/api/statistics/overview", "/api/campaigns", "/api/qrcodes/1" };

        foreach (var testPath in testPaths)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = testPath;

            _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.That(context.Items["RequestPath"], Is.EqualTo(testPath));
        }
    }

    #endregion
}

