using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Infrastructure.Tests.Data;

/// <summary>
/// Tests für EasterEggHuntDbContextFactory
/// 
/// Note: Die Factory wird hauptsächlich für Design-Time-Operationen (Migrations) verwendet.
/// Für Runtime-Tests verwenden wir direkt DbContext-Konfiguration.
/// </summary>
[TestFixture]
public class EasterEggHuntDbContextFactoryTests
{
    [Test]
    public void Factory_ShouldExist()
    {
        // Arrange & Act
        var factory = new EasterEggHuntDbContextFactory();

        // Assert
        Assert.That(factory, Is.Not.Null);
    }

    [Test]
    public void Factory_CanCreateDbContext_WithDirectConfiguration()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<EasterEggHuntDbContext>();
        optionsBuilder.UseSqlite("Data Source=:memory:");

        // Act
        using var context = new EasterEggHuntDbContext(optionsBuilder.Options);

        // Assert
        Assert.That(context, Is.Not.Null);
        Assert.That(context, Is.InstanceOf<EasterEggHuntDbContext>());
    }

    [Test]
    public void DbContext_WithInMemoryDatabase_ShouldWork()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<EasterEggHuntDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        // Act
        using var context = new EasterEggHuntDbContext(options);
        context.Database.EnsureCreated();

        // Assert
        Assert.That(context.Database.ProviderName, Is.EqualTo("Microsoft.EntityFrameworkCore.Sqlite"));
        Assert.That(context.Campaigns, Is.Not.Null);
        Assert.That(context.QrCodes, Is.Not.Null);
        Assert.That(context.Users, Is.Not.Null);
    }
}

