using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Tests für den CampaignService
/// </summary>
[TestFixture]
public class CampaignServiceTests
{
    [Test]
    public void CampaignService_Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<CampaignService>>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CampaignService(null!, logger));
        Assert.That(ex.ParamName, Is.EqualTo("campaignRepository"));
    }

    [Test]
    public void CampaignService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var repository = new Mock<ICampaignRepository>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CampaignService(repository, null!));
        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public void CampaignService_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var repository = new Mock<ICampaignRepository>().Object;
        var logger = new Mock<ILogger<CampaignService>>().Object;

        // Act
        var service = new CampaignService(repository, logger);

        // Assert
        Assert.That(service, Is.Not.Null);
    }
}
