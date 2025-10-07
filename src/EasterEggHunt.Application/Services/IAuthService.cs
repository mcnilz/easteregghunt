using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für Authentifizierungs-Operationen
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authentifiziert einen Administrator
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <param name="password">Passwort</param>
    /// <returns>AdminUser wenn erfolgreich, null wenn fehlgeschlagen</returns>
    Task<AdminUser?> AuthenticateAdminAsync(string username, string password);

    /// <summary>
    /// Erstellt einen neuen Administrator
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <param name="email">E-Mail-Adresse</param>
    /// <param name="password">Passwort (wird gehasht)</param>
    /// <returns>Erstellter AdminUser</returns>
    Task<AdminUser> CreateAdminAsync(string username, string email, string password);

    /// <summary>
    /// Ändert das Passwort eines Administrators
    /// </summary>
    /// <param name="adminId">ID des Administrators</param>
    /// <param name="currentPassword">Aktuelles Passwort</param>
    /// <param name="newPassword">Neues Passwort</param>
    /// <returns>True wenn erfolgreich</returns>
    Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword);

    /// <summary>
    /// Generiert einen Passwort-Hash
    /// </summary>
    /// <param name="password">Passwort</param>
    /// <returns>Gehashtes Passwort</returns>
    string HashPassword(string password);

    /// <summary>
    /// Überprüft ein Passwort gegen einen Hash
    /// </summary>
    /// <param name="password">Passwort</param>
    /// <param name="hash">Hash</param>
    /// <returns>True wenn Passwort korrekt</returns>
    bool VerifyPassword(string password, string hash);
}
