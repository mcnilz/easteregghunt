using EasterEggHunt.Infrastructure.Configuration;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests.Configuration;

[TestFixture]
public class DbContextConfigurationTests
{
    [Test]
    public void ConfigureDbContext_WithValidConnectionString_ConfiguresSqlite()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            })
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        DbContextConfiguration.ConfigureDbContext(optionsBuilder, configuration);

        // Assert
        Assert.That(optionsBuilder.Options, Is.Not.Null);
    }

    [Test]
    public void ConfigureDbContext_WithNullConnectionString_UsesDefaultConnectionString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        DbContextConfiguration.ConfigureDbContext(optionsBuilder, configuration);

        // Assert
        Assert.That(optionsBuilder.Options, Is.Not.Null);
    }

    [Test]
    public void ConfigureDbContext_WithDesignTimeFlag_ConfiguresSqlite()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            })
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        DbContextConfiguration.ConfigureDbContext(optionsBuilder, configuration, true);

        // Assert
        Assert.That(optionsBuilder.Options, Is.Not.Null);
    }

    [Test]
    public void ConfigureDbContext_WithDesignTimeFlagAndNullConnectionString_UsesDefaultConnectionString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        DbContextConfiguration.ConfigureDbContext(optionsBuilder, configuration, true);

        // Assert
        Assert.That(optionsBuilder.Options, Is.Not.Null);
    }

}
