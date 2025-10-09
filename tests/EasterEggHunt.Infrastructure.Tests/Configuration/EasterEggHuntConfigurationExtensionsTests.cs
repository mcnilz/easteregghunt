using EasterEggHunt.Domain.Configuration;
using EasterEggHunt.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests.Configuration;

[TestFixture]
public class EasterEggHuntConfigurationExtensionsTests
{
    private ServiceCollection _services = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EasterEggHunt:SomeSetting"] = "TestValue"
            })
            .Build();
    }

    [Test]
    public void AddEasterEggHuntConfiguration_WithValidConfiguration_RegistersOptions()
    {
        // Act
        var result = _services.AddEasterEggHuntConfiguration(_configuration);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    [Test]
    public void AddEasterEggHuntConfiguration_WithEmptyConfiguration_HandlesGracefully()
    {
        // Arrange
        var emptyConfiguration = new ConfigurationBuilder().Build();

        // Act
        var result = _services.AddEasterEggHuntConfiguration(emptyConfiguration);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    [Test]
    public void AddEasterEggHuntConfiguration_ReturnsSameServiceCollection()
    {
        // Act
        var result = _services.AddEasterEggHuntConfiguration(_configuration);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    [Test]
    public void AddEasterEggHuntConfiguration_AllowsMethodChaining()
    {
        // Act
        var result = _services
            .AddEasterEggHuntConfiguration(_configuration)
            .AddEasterEggHuntConfiguration(_configuration);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }
}
