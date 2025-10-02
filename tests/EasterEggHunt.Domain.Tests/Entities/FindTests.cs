using EasterEggHunt.Domain.Entities;
using FluentAssertions;
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
        find.QrCodeId.Should().Be(ValidQrCodeId);
        find.UserId.Should().Be(ValidUserId);
        find.IpAddress.Should().Be(ValidIpAddress);
        find.UserAgent.Should().Be(ValidUserAgent);
        find.FoundAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Constructor_WithNullIpAddress_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new Find(ValidQrCodeId, ValidUserId, null!, ValidUserAgent);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("ipAddress");
    }

    [Test]
    public void Constructor_WithNullUserAgent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userAgent");
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
        find.FoundAt.Should().BeOnOrAfter(beforeCreation);
        find.FoundAt.Should().BeOnOrBefore(afterCreation);
    }
}
