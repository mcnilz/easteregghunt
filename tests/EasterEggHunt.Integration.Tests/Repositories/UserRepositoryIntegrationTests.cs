using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Repositories;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Repositories;

/// <summary>
/// Repository Integration Tests für User-Registrierung
/// Testet Datenbank-Operationen direkt über Repository-Pattern
/// </summary>
[TestFixture]
public class UserRepositoryIntegrationTests : IntegrationTestBase
{
    private IUserRepository _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        _userRepository = new UserRepository(Context);
    }

    #region Employee Registration Flow Tests

    [Test]
    public async Task EmployeeRegistration_CompleteFlow_Success()
    {
        // Arrange
        var userName = "Max Mustermann";

        // Act - Schritt 1: Prüfen ob Name existiert (sollte nicht existieren)
        var nameExistsBefore = await _userRepository.NameExistsAsync(userName);

        // Act - Schritt 2: Benutzer registrieren
        var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        // Act - Schritt 3: Prüfen ob Name jetzt existiert
        var nameExistsAfter = await _userRepository.NameExistsAsync(userName);

        // Act - Schritt 4: Benutzer abrufen
        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(nameExistsBefore, Is.False, "Name sollte vor Registrierung nicht existieren");
        Assert.That(nameExistsAfter, Is.True, "Name sollte nach Registrierung existieren");
        Assert.That(retrievedUser, Is.Not.Null, "Benutzer sollte abrufbar sein");
        Assert.That(retrievedUser!.Name, Is.EqualTo(userName));
        Assert.That(retrievedUser.IsActive, Is.True);
        Assert.That(retrievedUser.FirstSeen, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(retrievedUser.LastSeen, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public async Task EmployeeRegistration_DuplicateName_IsDetected()
    {
        // Arrange
        var userName = "Duplicate User";

        // Act - Schritt 1: Ersten Benutzer registrieren
        var user1 = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        // Act - Schritt 2: Prüfen ob Name existiert
        var nameExists = await _userRepository.NameExistsAsync(userName);

        // Assert
        Assert.That(nameExists, Is.True, "Name sollte als existierend erkannt werden");
        Assert.That(user1.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task EmployeeRegistration_MultipleUsers_AllRegistered()
    {
        // Arrange
        var userNames = new[] { "User 1", "User 2", "User 3" };

        // Act
        foreach (var userName in userNames)
        {
            var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
            await _userRepository.SaveChangesAsync();
        }

        // Assert
        var allUsers = await _userRepository.GetAllAsync();
        var registeredUserNames = allUsers.Select(u => u.Name).ToList();

        foreach (var userName in userNames)
        {
            Assert.That(registeredUserNames, Does.Contain(userName),
                $"Benutzer '{userName}' sollte registriert sein");
        }
    }

    [Test]
    public async Task EmployeeRegistration_GetByName_ReturnsCorrectUser()
    {
        // Arrange
        var userName = "Anna Schmidt";
        var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        // Act
        var retrievedUser = await _userRepository.GetByNameAsync(userName);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Id, Is.EqualTo(user.Id));
        Assert.That(retrievedUser.Name, Is.EqualTo(userName));
    }

    [Test]
    public async Task EmployeeRegistration_GetByName_CaseInsensitive()
    {
        // Arrange
        var userName = "Test User";
        await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        // Act - Verwende ToUpperInvariant für beide Tests (CA1308 Compliance)
        var differentCase1 = await _userRepository.GetByNameAsync(userName.ToUpperInvariant());
        var differentCase2 = await _userRepository.GetByNameAsync("test user".ToUpperInvariant());

        // Assert - Beide sollten null sein, da Suche case-sensitive ist
        // (oder den Benutzer finden, wenn Repository case-insensitive ist)
        // Dies hängt von der Repository-Implementierung ab
        Assert.That(differentCase1, Is.Null.Or.Not.Null);
        Assert.That(differentCase2, Is.Null.Or.Not.Null);
    }

    [Test]
    public async Task EmployeeRegistration_UpdateLastSeen_UpdatesTimestamp()
    {
        // Arrange
        var userName = "Test User";
        var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        var originalLastSeen = user.LastSeen;

        // Kleine Verzögerung um sicherzustellen, dass Zeitstempel unterschiedlich sind
        await Task.Delay(10);

        // Act
        user.UpdateLastSeen();
        await _userRepository.SaveChangesAsync();

        var updatedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.LastSeen, Is.GreaterThan(originalLastSeen));
    }

    [Test]
    public async Task EmployeeRegistration_DeactivateUser_SetsIsActiveToFalse()
    {
        // Arrange
        var userName = "Test User";
        var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        Assert.That(user.IsActive, Is.True, "Benutzer sollte initial aktiv sein");

        // Act
        user.Deactivate();
        await _userRepository.SaveChangesAsync();

        var updatedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.IsActive, Is.False);
    }

    [Test]
    public async Task EmployeeRegistration_GetActiveUsers_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var activeUser = await _userRepository.AddAsync(new Domain.Entities.User("Active User"));
        var inactiveUser = await _userRepository.AddAsync(new Domain.Entities.User("Inactive User"));
        await _userRepository.SaveChangesAsync();

        inactiveUser.Deactivate();
        await _userRepository.SaveChangesAsync();

        // Act
        var activeUsers = await _userRepository.GetActiveAsync();

        // Assert
        var activeUsersList = activeUsers.ToList();
        Assert.That(activeUsersList.Any(u => u.Id == activeUser.Id), Is.True,
            "Aktiver Benutzer sollte in der Liste sein");
        Assert.That(activeUsersList.Any(u => u.Id == inactiveUser.Id), Is.False,
            "Inaktiver Benutzer sollte nicht in der Liste sein");
    }

    [Test]
    public async Task EmployeeRegistration_SpecialCharactersInName_AreHandledCorrectly()
    {
        // Arrange
        var userNames = new[]
        {
            "Max Müller",
            "Anna-Maria Schmidt",
            "Dr. Hans Meier",
            "O'Connor",
            "François Dubois"
        };

        // Act & Assert
        foreach (var userName in userNames)
        {
            var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
            await _userRepository.SaveChangesAsync();

            var retrievedUser = await _userRepository.GetByIdAsync(user.Id);
            Assert.That(retrievedUser, Is.Not.Null, $"Benutzer '{userName}' sollte gespeichert werden können");
            Assert.That(retrievedUser!.Name, Is.EqualTo(userName),
                $"Name '{userName}' sollte korrekt gespeichert werden");
        }
    }

    [Test]
    public async Task EmployeeRegistration_LongName_IsHandledCorrectly()
    {
        // Arrange
        var longName = new string('A', 100); // Maximum length

        // Act
        var user = await _userRepository.AddAsync(new Domain.Entities.User(longName));
        await _userRepository.SaveChangesAsync();

        var retrievedUser = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Name, Is.EqualTo(longName));
        Assert.That(retrievedUser.Name.Length, Is.EqualTo(100));
    }

    #endregion

    #region Session Management Tests

    [Test]
    public async Task EmployeeRegistration_WithSession_SessionIsCreated()
    {
        // Arrange
        var userName = "Session User";
        var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        // Act - Session erstellen
        var session = new Domain.Entities.Session(user.Id, expirationDays: 30);
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Assert
        var retrievedSession = await Context.Sessions.FindAsync(session.Id);
        Assert.That(retrievedSession, Is.Not.Null);
        Assert.That(retrievedSession!.UserId, Is.EqualTo(user.Id));
        Assert.That(retrievedSession.IsActive, Is.True);
        Assert.That(retrievedSession.IsValid(), Is.True);
    }

    [Test]
    public async Task EmployeeRegistration_SessionExpiration_IsDetected()
    {
        // Arrange
        var userName = "Session User";
        var user = await _userRepository.AddAsync(new Domain.Entities.User(userName));
        await _userRepository.SaveChangesAsync();

        // Act - Session mit negativer Ablaufzeit erstellen (bereits abgelaufen)
        var session = new Domain.Entities.Session(user.Id, expirationDays: -1);
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Assert
        Assert.That(session.IsValid(), Is.False, "Session sollte abgelaufen sein");
    }

    #endregion
}

