using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure;
using EasterEggHunt.Infrastructure.Configuration;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private ServiceCollection _services = null!;
    private IConfiguration _configuration = null!;
    private IHostEnvironment _environment = null!;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            })
            .Build();

        _environment = new TestHostEnvironment { EnvironmentName = "Development" };
    }

    [Test]
    public void AddEasterEggHuntDbContext_WithConfiguration_RegistersDbContext()
    {
        // Act
        var result = _services.AddEasterEggHuntDbContext(_configuration);

        // Assert
        Assert.That(result, Is.SameAs(_services));

        // Verify DbContext is registered
        var serviceProvider = _services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<EasterEggHuntDbContext>();
        Assert.That(dbContext, Is.Not.Null);
    }

    [Test]
    public void AddEasterEggHuntDbContext_WithConfigureOptions_RegistersDbContext()
    {
        // Arrange
        Action<DbContextOptionsBuilder> configureOptions = options =>
            options.UseSqlite("Data Source=test.db");

        // Act
        var result = _services.AddEasterEggHuntDbContext(configureOptions);

        // Assert
        Assert.That(result, Is.SameAs(_services));

        // Verify DbContext is registered
        var serviceProvider = _services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<EasterEggHuntDbContext>();
        Assert.That(dbContext, Is.Not.Null);
    }

    [Test]
    public void AddRepositories_RegistersAllRepositories()
    {
        // Arrange - Add required dependencies first
        _services.AddEasterEggHuntDbContext(_configuration);
        _services.AddLogging();

        // Act
        var result = _services.AddRepositories();

        // Assert
        Assert.That(result, Is.SameAs(_services));

        // Verify all repositories are registered
        var serviceProvider = _services.BuildServiceProvider();

        Assert.That(serviceProvider.GetService<ICampaignRepository>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IQrCodeRepository>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IUserRepository>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IFindRepository>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<ISessionRepository>(), Is.Not.Null);
        Assert.That(serviceProvider.GetService<IAdminUserRepository>(), Is.Not.Null);
    }

    [Test]
    public void AddSeedDataService_WithDevelopmentEnvironment_RegistersSeedDataService()
    {
        // Arrange
        _services.AddLogging();
        _services.AddSingleton(_configuration);

        // Act
        var result = _services.AddSeedDataService(_environment);

        // Assert
        Assert.That(result, Is.SameAs(_services));

        // Verify SeedDataService is registered as hosted service
        var serviceProvider = _services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.That(hostedServices.Any(s => s is SeedDataService), Is.True);
    }

    [Test]
    public void AddSeedDataService_WithProductionEnvironment_DoesNotRegisterSeedDataService()
    {
        // Arrange
        var productionEnvironment = new TestHostEnvironment { EnvironmentName = "Production" };
        _services.AddLogging();
        _services.AddSingleton(_configuration);

        // Act
        var result = _services.AddSeedDataService(productionEnvironment);

        // Assert
        Assert.That(result, Is.SameAs(_services));

        // Verify SeedDataService is NOT registered
        var serviceProvider = _services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.That(hostedServices.Any(s => s is SeedDataService), Is.False);
    }

    [Test]
    public void AddSeedDataService_WithCaseInsensitiveEnvironmentName_RegistersSeedDataService()
    {
        // Arrange
        var devEnvironment = new TestHostEnvironment { EnvironmentName = "DEVELOPMENT" };
        _services.AddLogging();
        _services.AddSingleton(_configuration);

        // Act
        var result = _services.AddSeedDataService(devEnvironment);

        // Assert
        Assert.That(result, Is.SameAs(_services));

        // Verify SeedDataService is registered
        var serviceProvider = _services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();
        Assert.That(hostedServices.Any(s => s is SeedDataService), Is.True);
    }

    [Test]
    public void AllMethods_AllowMethodChaining()
    {
        // Arrange
        _services.AddLogging();
        _services.AddSingleton(_configuration);

        // Act
        var result = _services
            .AddEasterEggHuntDbContext(_configuration)
            .AddRepositories()
            .AddSeedDataService(_environment);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
