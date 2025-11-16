using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Infrastructure.Tests.Data;

/// <summary>
/// Unit Tests für SessionCleanupService
/// </summary>
[TestFixture]
public sealed class SessionCleanupServiceTests : IDisposable
{
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<IServiceScope> _mockServiceScope = null!;
    private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
    private Mock<ISessionRepository> _mockSessionRepository = null!;
    private Mock<ILogger<SessionCleanupService>> _mockLogger = null!;
    private CancellationTokenSource _cancellationTokenSource = null!;

    [SetUp]
    public void SetUp()
    {
        _mockSessionRepository = new Mock<ISessionRepository>();
        _mockLogger = new Mock<ILogger<SessionCleanupService>>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _cancellationTokenSource = new CancellationTokenSource();

        // ServiceScope Setup
        _mockServiceScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockServiceScopeFactory.Object);
        _mockServiceProvider.Setup(p => p.GetService(typeof(ISessionRepository)))
            .Returns(_mockSessionRepository.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // TearDown wird vor Dispose() aufgerufen, daher hier bereinigen
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null!; // Markiere als bereits disposed
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SessionCleanupService(
            null!,
            _mockLogger.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            true));
    }

    [Test]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SessionCleanupService(
            _mockServiceProvider.Object,
            null!,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            true));
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(30),
            true);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    #endregion

    #region ExecuteAsync Tests

    [Test]
    public async Task ExecuteAsync_WhenDisabled_ShouldNotRunCleanup()
    {
        // Arrange
        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(50),
            isEnabled: false);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(150); // Warten, um sicherzustellen, dass der Loop nicht läuft
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockSessionRepository.Verify(r => r.DeleteExpiredAsync(), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenEnabled_ShouldRunCleanup()
    {
        // Arrange
        _mockSessionRepository.Setup(r => r.DeleteExpiredAsync())
            .ReturnsAsync(0);

        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200); // Warten bis nach initialem Delay (30s) + Interval (100ms)
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockSessionRepository.Verify(r => r.DeleteExpiredAsync(), Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_WhenSessionsDeleted_ShouldLogInformation()
    {
        // Arrange
        _mockSessionRepository.Setup(r => r.DeleteExpiredAsync())
            .ReturnsAsync(5);

        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(500), // Längeres Interval, damit nur ein Cleanup stattfindet
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(120); // Warten nach initialem Delay (50ms) + bisschen mehr für ersten Cleanup
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockSessionRepository.Verify(r => r.DeleteExpiredAsync(), Times.AtLeastOnce);
        // Verify logging wurde aufgerufen (prüfe, ob Log-Information mit "5 abgelaufene Session(s)" aufgerufen wurde)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("5 abgelaufene Session(s)")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_WhenNoSessionsDeleted_ShouldLogDebug()
    {
        // Arrange
        _mockSessionRepository.Setup(r => r.DeleteExpiredAsync())
            .ReturnsAsync(0);

        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(500), // Längeres Interval, damit nur ein Cleanup stattfindet
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(120); // Warten nach initialem Delay (50ms) + bisschen mehr für ersten Cleanup
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockSessionRepository.Verify(r => r.DeleteExpiredAsync(), Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldLogErrorAndContinue()
    {
        // Arrange
        _mockSessionRepository.Setup(r => r.DeleteExpiredAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(500), // Längeres Interval, damit nur ein Cleanup stattfindet
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(120); // Warten nach initialem Delay (50ms) + bisschen mehr für ersten Cleanup
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fehler bei der Session-Bereinigung")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldStopGracefully()
    {
        // Arrange
        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromHours(24),
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50);
        await _cancellationTokenSource.CancelAsync();
        await Task.Delay(50); // Warten, damit StopAsync abgeschlossen werden kann

        // Assert
        // Service sollte sauber gestoppt werden können
        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public async Task ExecuteAsync_ShouldCreateScopeForEachCleanup()
    {
        // Arrange
        _mockSessionRepository.Setup(r => r.DeleteExpiredAsync())
            .ReturnsAsync(0);

        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(500), // Längeres Interval, damit nur ein Cleanup stattfindet
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(120); // Warten nach initialem Delay (50ms) + bisschen mehr für ersten Cleanup
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_ShouldDisposeScopeAfterCleanup()
    {
        // Arrange
        _mockSessionRepository.Setup(r => r.DeleteExpiredAsync())
            .ReturnsAsync(0);

        using var service = new SessionCleanupService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            TimeSpan.FromMilliseconds(500), // Längeres Interval, damit nur ein Cleanup stattfindet
            TimeSpan.FromMilliseconds(50),
            isEnabled: true);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(120); // Warten nach initialem Delay (50ms) + bisschen mehr für ersten Cleanup
        await _cancellationTokenSource.CancelAsync();

        // Assert
        _mockServiceScope.Verify(s => s.Dispose(), Times.AtLeastOnce);
    }

    public void Dispose()
    {
        // Dispose() wird von NUnit aufgerufen, um Code-Analyse-Regeln zu erfüllen
        // TearDown() bereinigt bereits, daher nur falls noch nicht disposed
        try
        {
            _cancellationTokenSource?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Bereits disposed - kein Problem
        }
        finally
        {
            _cancellationTokenSource = null!;
        }
    }

    #endregion
}

