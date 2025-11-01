using EasterEggHunt.Domain.Entities;

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

    [Test]
    public void Constructor_WithZeroIds_ShouldCreateFind()
    {
        // Act
        var find = new Find(0, 0, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.QrCodeId, Is.EqualTo(0));
        Assert.That(find.UserId, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithNegativeIds_ShouldCreateFind()
    {
        // Act
        var find = new Find(-1, -1, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.QrCodeId, Is.EqualTo(-1));
        Assert.That(find.UserId, Is.EqualTo(-1));
    }

    [Test]
    public void Constructor_WithEmptyStrings_ShouldCreateFind()
    {
        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, string.Empty, string.Empty);

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo(string.Empty));
        Assert.That(find.UserAgent, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constructor_WithIPv6Address_ShouldCreateFind()
    {
        // Arrange
        var ipv6Address = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ipv6Address, ValidUserAgent);

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo(ipv6Address));
    }

    [Test]
    public void Constructor_WithLocalhostAddress_ShouldCreateFind()
    {
        // Arrange
        var localhostAddress = "127.0.0.1";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, localhostAddress, ValidUserAgent);

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo(localhostAddress));
    }

    [Test]
    public void Constructor_WithVeryLongIpAddress_ShouldCreateFind()
    {
        // Arrange - IPv6 mit Port kann sehr lang sein
        var longIpAddress = "[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:8080";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, longIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo(longIpAddress));
    }

    [Test]
    public void Constructor_WithVeryLongUserAgent_ShouldCreateFind()
    {
        // Arrange - User-Agents können sehr lang sein
        var longUserAgent = new string('A', 500) + " Browser/1.0";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, longUserAgent);

        // Assert
        Assert.That(find.UserAgent, Is.EqualTo(longUserAgent));
    }

    [Test]
    public void Constructor_WithSpecialCharactersInUserAgent_ShouldCreateFind()
    {
        // Arrange
        var specialUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, specialUserAgent);

        // Assert
        Assert.That(find.UserAgent, Is.EqualTo(specialUserAgent));
    }

    [Test]
    public void Constructor_WithUnicodeCharactersInUserAgent_ShouldCreateFind()
    {
        // Arrange
        var unicodeUserAgent = "Mozilla/5.0 (中文 Browser; 🎉 Test)";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, unicodeUserAgent);

        // Assert
        Assert.That(find.UserAgent, Is.EqualTo(unicodeUserAgent));
    }

    [Test]
    public void Constructor_WithMultipleSameUserAgents_ShouldCreateDistinctFinds()
    {
        // Act
        var find1 = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);
        Thread.Sleep(10);
        var find2 = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find1.FoundAt, Is.Not.EqualTo(find2.FoundAt));
        Assert.That(find2.FoundAt, Is.GreaterThan(find1.FoundAt));
    }

    [Test]
    public void Constructor_WithSameIdsButDifferentTimes_ShouldCreateDifferentFinds()
    {
        // Act
        var find1 = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);
        Thread.Sleep(10);
        var find2 = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find1.QrCodeId, Is.EqualTo(find2.QrCodeId));
        Assert.That(find1.UserId, Is.EqualTo(find2.UserId));
        Assert.That(find1.FoundAt, Is.Not.EqualTo(find2.FoundAt));
    }

    [Test]
    public void Constructor_WithWhitespaceStrings_ShouldCreateFind()
    {
        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, "   ", "   ");

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo("   "));
        Assert.That(find.UserAgent, Is.EqualTo("   "));
    }

    [Test]
    public void Constructor_WithMaxIntValues_ShouldCreateFind()
    {
        // Act
        var find = new Find(int.MaxValue, int.MaxValue, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.QrCodeId, Is.EqualTo(int.MaxValue));
        Assert.That(find.UserId, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Constructor_WithMinIntValues_ShouldCreateFind()
    {
        // Act
        var find = new Find(int.MinValue, int.MinValue, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.QrCodeId, Is.EqualTo(int.MinValue));
        Assert.That(find.UserId, Is.EqualTo(int.MinValue));
    }

    [Test]
    public void Constructor_FoundAtShouldBeUtcTime()
    {
        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.FoundAt.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void Constructor_WithEmptyIpAddress_ShouldCreateFind()
    {
        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, string.Empty, ValidUserAgent);

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo(string.Empty));
        Assert.That(find.QrCodeId, Is.EqualTo(ValidQrCodeId));
        Assert.That(find.UserId, Is.EqualTo(ValidUserId));
    }

    [Test]
    public void Constructor_WithEmptyUserAgent_ShouldCreateFind()
    {
        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, string.Empty);

        // Assert
        Assert.That(find.UserAgent, Is.EqualTo(string.Empty));
        Assert.That(find.QrCodeId, Is.EqualTo(ValidQrCodeId));
        Assert.That(find.UserId, Is.EqualTo(ValidUserId));
    }

    [Test]
    public void Constructor_WithInvalidIpAddressFormat_ShouldCreateFind()
    {
        // Arrange - Domain Entity akzeptiert jeden String, Validierung sollte auf Service-Ebene sein
        var invalidIpAddress = "not.an.ip.address";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, invalidIpAddress, ValidUserAgent);

        // Assert
        Assert.That(find.IpAddress, Is.EqualTo(invalidIpAddress));
    }

    [Test]
    public void Constructor_WithNewlinesInUserAgent_ShouldCreateFind()
    {
        // Arrange
        var userAgentWithNewlines = "Mozilla/5.0\n(Windows NT 10.0)\r\nTest Browser";

        // Act
        var find = new Find(ValidQrCodeId, ValidUserId, ValidIpAddress, userAgentWithNewlines);

        // Assert
        Assert.That(find.UserAgent, Is.EqualTo(userAgentWithNewlines));
    }
}
