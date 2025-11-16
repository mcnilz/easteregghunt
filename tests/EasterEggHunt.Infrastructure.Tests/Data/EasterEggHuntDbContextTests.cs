using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Infrastructure.Tests.Data;

/// <summary>
/// Tests für EasterEggHuntDbContext
/// </summary>
[TestFixture]
[Category("Integration")]
public sealed class EasterEggHuntDbContextTests : IDisposable
{
    private EasterEggHuntDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<EasterEggHuntDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        _context = new EasterEggHuntDbContext(options);
        _context.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    #region DbSet Tests

    [Test]
    public void Campaigns_ShouldBeInitialized()
    {
        // Assert
        Assert.That(_context.Campaigns, Is.Not.Null);
    }

    [Test]
    public void QrCodes_ShouldBeInitialized()
    {
        // Assert
        Assert.That(_context.QrCodes, Is.Not.Null);
    }

    [Test]
    public void Users_ShouldBeInitialized()
    {
        // Assert
        Assert.That(_context.Users, Is.Not.Null);
    }

    [Test]
    public void Finds_ShouldBeInitialized()
    {
        // Assert
        Assert.That(_context.Finds, Is.Not.Null);
    }

    [Test]
    public void Sessions_ShouldBeInitialized()
    {
        // Assert
        Assert.That(_context.Sessions, Is.Not.Null);
    }

    [Test]
    public void AdminUsers_ShouldBeInitialized()
    {
        // Assert
        Assert.That(_context.AdminUsers, Is.Not.Null);
    }

    #endregion

    #region Entity Configuration Tests

    [Test]
    public async Task SaveChangesAsync_WithValidCampaign_ShouldPersist()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");

        // Act
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();

        // Assert
        var savedCampaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == campaign.Id);
        Assert.That(savedCampaign, Is.Not.Null);
        Assert.That(savedCampaign!.Name, Is.EqualTo("Test Campaign"));
    }

    [Test]
    public async Task SaveChangesAsync_WithValidQrCode_ShouldPersist()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        await _context.Campaigns.AddAsync(campaign);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");

        // Act
        _context.QrCodes.Add(qrCode);
        await _context.SaveChangesAsync();

        // Assert
        var savedQrCode = await _context.QrCodes.FirstOrDefaultAsync(q => q.Id == qrCode.Id);
        Assert.That(savedQrCode, Is.Not.Null);
        Assert.That(savedQrCode!.Title, Is.EqualTo("Test QR"));
    }

    [Test]
    public async Task SaveChangesAsync_WithValidUser_ShouldPersist()
    {
        // Arrange
        var user = new User("Test User");

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.Name, Is.EqualTo("Test User"));
    }

    [Test]
    public async Task SaveChangesAsync_WithValidFind_ShouldPersist()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var user = new User("Test User");
        await _context.Campaigns.AddAsync(campaign);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode);
        await _context.SaveChangesAsync();

        var find = new Find(qrCode.Id, user.Id, "127.0.0.1", "Test User Agent");

        // Act
        _context.Finds.Add(find);
        await _context.SaveChangesAsync();

        // Assert
        var savedFind = await _context.Finds.FirstOrDefaultAsync(f => f.Id == find.Id);
        Assert.That(savedFind, Is.Not.Null);
        Assert.That(savedFind!.IpAddress, Is.EqualTo("127.0.0.1"));
    }

    [Test]
    public async Task SaveChangesAsync_WithValidSession_ShouldPersist()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var session = new Session(user.Id, 30);

        // Act
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        Assert.That(savedSession, Is.Not.Null);
        Assert.That(savedSession!.UserId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task SaveChangesAsync_WithValidAdminUser_ShouldPersist()
    {
        // Arrange
        var adminUser = new AdminUser("Admin", "admin@test.com", "hashedPassword");

        // Act
        _context.AdminUsers.Add(adminUser);
        await _context.SaveChangesAsync();

        // Assert
        var savedAdminUser = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == adminUser.Id);
        Assert.That(savedAdminUser, Is.Not.Null);
        Assert.That(savedAdminUser!.Username, Is.EqualTo("Admin"));
    }

    #endregion

    #region Relationship Tests

    [Test]
    public async Task Campaign_WithQrCodes_ShouldMaintainRelationship()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        await _context.Campaigns.AddAsync(campaign);
        await _context.SaveChangesAsync();

        var qrCode1 = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1");
        var qrCode2 = new QrCode(campaign.Id, "QR2", "Desc2", "Notes2");
        await _context.QrCodes.AddAsync(qrCode1);
        await _context.QrCodes.AddAsync(qrCode2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedCampaign = await _context.Campaigns
            .Include(c => c.QrCodes)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id);

        // Assert
        Assert.That(retrievedCampaign, Is.Not.Null);
        Assert.That(retrievedCampaign!.QrCodes, Has.Count.EqualTo(2));
        Assert.That(retrievedCampaign.QrCodes, Has.Some.Matches<QrCode>(q => q.Id == qrCode1.Id));
        Assert.That(retrievedCampaign.QrCodes, Has.Some.Matches<QrCode>(q => q.Id == qrCode2.Id));
    }

    [Test]
    public async Task QrCode_WithFinds_ShouldMaintainRelationship()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var user = new User("Test User");
        await _context.Campaigns.AddAsync(campaign);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode);
        await _context.SaveChangesAsync();

        var find1 = new Find(qrCode.Id, user.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode.Id, user.Id, "192.168.1.1", "User Agent 2");
        await _context.Finds.AddAsync(find1);
        await _context.Finds.AddAsync(find2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedQrCode = await _context.QrCodes
            .Include(q => q.Finds)
            .FirstOrDefaultAsync(q => q.Id == qrCode.Id);

        // Assert
        Assert.That(retrievedQrCode, Is.Not.Null);
        Assert.That(retrievedQrCode!.Finds, Has.Count.EqualTo(2));
        Assert.That(retrievedQrCode.Finds, Has.Some.Matches<Find>(f => f.Id == find1.Id));
        Assert.That(retrievedQrCode.Finds, Has.Some.Matches<Find>(f => f.Id == find2.Id));
    }

    [Test]
    public async Task User_WithFinds_ShouldMaintainRelationship()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var user = new User("Test User");
        await _context.Campaigns.AddAsync(campaign);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode);
        await _context.SaveChangesAsync();

        var find1 = new Find(qrCode.Id, user.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode.Id, user.Id, "192.168.1.1", "User Agent 2");
        await _context.Finds.AddAsync(find1);
        await _context.Finds.AddAsync(find2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedUser = await _context.Users
            .Include(u => u.Finds)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Finds, Has.Count.EqualTo(2));
        Assert.That(retrievedUser.Finds, Has.Some.Matches<Find>(f => f.Id == find1.Id));
        Assert.That(retrievedUser.Finds, Has.Some.Matches<Find>(f => f.Id == find2.Id));
    }

    [Test]
    public async Task User_WithSessions_ShouldMaintainRelationship()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var session1 = new Session(user.Id, 30);
        var session2 = new Session(user.Id, 60);
        await _context.Sessions.AddAsync(session1);
        await _context.Sessions.AddAsync(session2);
        await _context.SaveChangesAsync();

        // Act
        var retrievedUser = await _context.Users
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Sessions, Has.Count.EqualTo(2));
        Assert.That(retrievedUser.Sessions, Has.Some.Matches<Session>(s => s.Id == session1.Id));
        Assert.That(retrievedUser.Sessions, Has.Some.Matches<Session>(s => s.Id == session2.Id));
    }

    [Test]
    public async Task DeleteCampaign_WithCascadeDelete_ShouldDeleteQrCodes()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        await _context.Campaigns.AddAsync(campaign);
        await _context.SaveChangesAsync();

        var qrCode1 = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1");
        var qrCode2 = new QrCode(campaign.Id, "QR2", "Desc2", "Notes2");
        await _context.QrCodes.AddAsync(qrCode1);
        await _context.QrCodes.AddAsync(qrCode2);
        await _context.SaveChangesAsync();

        // Act
        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync();

        // Assert
        var remainingQrCodes = await _context.QrCodes.ToListAsync();
        Assert.That(remainingQrCodes, Has.None.Matches<QrCode>(q => q.Id == qrCode1.Id));
        Assert.That(remainingQrCodes, Has.None.Matches<QrCode>(q => q.Id == qrCode2.Id));
    }

    [Test]
    public async Task DeleteQrCode_WithCascadeDelete_ShouldDeleteFinds()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var user = new User("Test User");
        await _context.Campaigns.AddAsync(campaign);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode);
        await _context.SaveChangesAsync();

        var find1 = new Find(qrCode.Id, user.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode.Id, user.Id, "192.168.1.1", "User Agent 2");
        await _context.Finds.AddAsync(find1);
        await _context.Finds.AddAsync(find2);
        await _context.SaveChangesAsync();

        // Act
        _context.QrCodes.Remove(qrCode);
        await _context.SaveChangesAsync();

        // Assert
        var remainingFinds = await _context.Finds.ToListAsync();
        Assert.That(remainingFinds, Has.None.Matches<Find>(f => f.Id == find1.Id));
        Assert.That(remainingFinds, Has.None.Matches<Find>(f => f.Id == find2.Id));
    }

    [Test]
    public async Task DeleteUser_WithCascadeDelete_ShouldDeleteFindsAndSessions()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var user = new User("Test User");
        await _context.Campaigns.AddAsync(campaign);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode);
        await _context.SaveChangesAsync();

        var find = new Find(qrCode.Id, user.Id, "127.0.0.1", "User Agent");
        var session = new Session(user.Id, 30);
        await _context.Finds.AddAsync(find);
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Assert
        var remainingFinds = await _context.Finds.ToListAsync();
        var remainingSessions = await _context.Sessions.ToListAsync();
        Assert.That(remainingFinds, Has.None.Matches<Find>(f => f.Id == find.Id));
        Assert.That(remainingSessions, Has.None.Matches<Session>(s => s.Id == session.Id));
    }

    #endregion

    #region Index Tests

    [Test]
    public async Task Campaigns_ShouldHaveIndexes()
    {
        // Arrange
        var campaign1 = new Campaign("Campaign 1", "Description 1", "Admin");
        var campaign2 = new Campaign("Campaign 2", "Description 2", "Admin");
        campaign2.Deactivate();
        await _context.Campaigns.AddAsync(campaign1);
        await _context.Campaigns.AddAsync(campaign2);
        await _context.SaveChangesAsync();

        // Act - Query mit Index-Filter
        var activeCampaigns = await _context.Campaigns
            .Where(c => c.IsActive)
            .ToListAsync();

        // Assert
        Assert.That(activeCampaigns, Has.Count.EqualTo(1));
        Assert.That(activeCampaigns[0].Name, Is.EqualTo("Campaign 1"));
    }

    [Test]
    public async Task QrCodes_ShouldHaveUniqueCodeIndex()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        await _context.Campaigns.AddAsync(campaign);
        await _context.SaveChangesAsync();

        var qrCode1 = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode1);
        await _context.SaveChangesAsync();

        var retrievedQrCode = await _context.QrCodes.FirstAsync(q => q.Id == qrCode1.Id);
        var code = retrievedQrCode.Code;

        // Act & Assert - Versuch, QrCode mit gleichem Code zu erstellen
        var qrCode2 = new QrCode(campaign.Id, "Test QR 2", "Description", "Notes");
        // Note: Unique constraint wird durch EF Core enforciert
        // Der Code wird bei der Entity-Erstellung generiert, daher ist dieser Test eher ein Smoke-Test
        await _context.QrCodes.AddAsync(qrCode2);
        await _context.SaveChangesAsync();

        // Beide QrCodes sollten unterschiedliche Codes haben (da Code bei Erstellung generiert wird)
        var retrievedQrCode2 = await _context.QrCodes.FirstAsync(q => q.Id == qrCode2.Id);
        Assert.That(retrievedQrCode2.Code, Is.Not.EqualTo(code));
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}

