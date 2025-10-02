using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Repositories;

namespace EasterEggHunt.Infrastructure.Tests.Repositories;

[TestFixture]
public class CampaignRepositoryTests
{
    private Mock<EasterEggHuntDbContext> _mockContext = null!;
    private Mock<DbSet<Campaign>> _mockDbSet = null!;
    private ICampaignRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<EasterEggHuntDbContext>();
        _mockDbSet = new Mock<DbSet<Campaign>>();
        _repository = new CampaignRepository(_mockContext.Object);
    }

    [Test]
    public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _ = new CampaignRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Test]
    public void Constructor_WithValidContext_ShouldCreateRepository()
    {
        // Arrange
        var context = new Mock<EasterEggHuntDbContext>();

        // Act
        var repository = new CampaignRepository(context.Object);

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeAssignableTo<ICampaignRepository>();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllCampaigns()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Description 1", "Admin"),
            new Campaign("Test Campaign 2", "Description 2", "Admin")
        };

        var queryable = campaigns.AsQueryable();
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Test Campaign 1");
        result.Should().Contain(c => c.Name == "Test Campaign 2");
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCampaign()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var campaigns = new List<Campaign> { campaign };

        var queryable = campaigns.AsQueryable();
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Campaign");
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var campaigns = new List<Campaign>();
        var queryable = campaigns.AsQueryable();
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task AddAsync_WithValidCampaign_ShouldAddCampaign()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _repository.AddAsync(campaign);

        // Assert
        result.Should().Be(campaign);
        _mockDbSet.Verify(d => d.Add(campaign), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddAsync_WithNullCampaign_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _repository.AddAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("campaign");
    }

    [Test]
    public async Task UpdateAsync_WithValidCampaign_ShouldUpdateCampaign()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(campaign);

        // Assert
        result.Should().Be(campaign);
        _mockDbSet.Verify(d => d.Update(campaign), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WithNullCampaign_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("campaign");
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteCampaign()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.Campaigns.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockDbSet.Verify(d => d.Remove(campaign), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);
        _mockContext.Setup(c => c.Campaigns.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign?)null);

        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockDbSet.Verify(d => d.Remove(It.IsAny<Campaign>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var campaigns = new List<Campaign> { new Campaign("Test", "Desc", "Admin") };
        var queryable = campaigns.AsQueryable();
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var campaigns = new List<Campaign>();
        var queryable = campaigns.AsQueryable();
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockDbSet.As<IQueryable<Campaign>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _mockContext.Setup(c => c.Campaigns).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task SaveChangesAsync_ShouldCallContextSaveChanges()
    {
        // Arrange
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(5);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
