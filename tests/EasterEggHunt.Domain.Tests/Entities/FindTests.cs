using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Tests für die Find Entity
/// </summary>
[TestFixture]
public class FindTests
{
    private const int ValidQrCodeId = 1;
    private const int ValidUserId = 1;
    private const string ValidIpAddress = "192.168.1.100";
    private const string ValidUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateFind()
    {
        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.QrCodeId, Is.EqualTo(ValidQrCodeId));
        Assert.That(find.UserId, Is.EqualTo(ValidUserId));
        Assert.That(find.IpAddress, Is.EqualTo(ValidIpAddress));
        Assert.That(find.UserAgent, Is.EqualTo(ValidUserAgent));
        Assert.That(find.FoundAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Constructor_WithNullIpAddress_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Find(ValidQrCodeId, ValidUserId, null!, ValidUserAgent));
        Assert.That(ex.ParamName, Is.EqualTo("ipAddress"));
    }

    [Test]
    public void Constructor_WithNullUserAgent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, null!));
        Assert.That(ex.ParamName, Is.EqualTo("userAgent"));
    }

    [Test]
    public void Constructor_ShouldSetFoundAtToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.That(find.FoundAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(find.FoundAt, Is.LessThanOrEqualTo(afterCreation));
    }
}
