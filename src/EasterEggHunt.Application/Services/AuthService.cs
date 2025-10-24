using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für Authentifizierungs-Operationen
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAdminUserRepository adminUserRepository, ILogger<AuthService> logger)
    {
        _adminUserRepository = adminUserRepository ?? throw new ArgumentNullException(nameof(adminUserRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authentifiziert einen Administrator
    /// </summary>
    public async Task<AdminUser?> AuthenticateAdminAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            _logger.LogInformation("Authenticating admin user: {Username}", username);

            var admin = await _adminUserRepository.GetByUsernameAsync(username);
            if (admin == null || !admin.IsActive)
            {
                _logger.LogWarning("Admin user not found or inactive: {Username}", username);
                return null;
            }

            if (!VerifyPassword(password, admin.PasswordHash))
            {
                _logger.LogWarning("Invalid password for admin user: {Username}", username);
                return null;
            }

            // Update last login
            admin.UpdateLastLogin();
            await _adminUserRepository.SaveAsync(admin);

            _logger.LogInformation("Admin user authenticated successfully: {Username}", username);
            return admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating admin user: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Erstellt einen neuen Administrator
    /// </summary>
    public async Task<AdminUser> CreateAdminAsync(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            _logger.LogInformation("Creating new admin user: {Username}", username);

            // Check if username already exists
            var existingAdmin = await _adminUserRepository.GetByUsernameAsync(username);
            if (existingAdmin != null)
            {
                throw new InvalidOperationException($"Admin user with username '{username}' already exists");
            }

            // Check if email already exists
            var existingEmail = await _adminUserRepository.GetByEmailAsync(email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Admin user with email '{email}' already exists");
            }

            var passwordHash = HashPassword(password);
            var admin = new AdminUser(username, email, passwordHash);

            await _adminUserRepository.SaveAsync(admin);

            _logger.LogInformation("Admin user created successfully: {Username}", username);
            return admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Ändert das Passwort eines Administrators
    /// </summary>
    public async Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            throw new ArgumentException("Current password cannot be null or empty", nameof(currentPassword));

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));

        try
        {
            _logger.LogInformation("Changing password for admin user: {AdminId}", adminId);

            var admin = await _adminUserRepository.GetByIdAsync(adminId);
            if (admin == null || !admin.IsActive)
            {
                _logger.LogWarning("Admin user not found or inactive: {AdminId}", adminId);
                return false;
            }

            if (!VerifyPassword(currentPassword, admin.PasswordHash))
            {
                _logger.LogWarning("Invalid current password for admin user: {AdminId}", adminId);
                return false;
            }

            var newPasswordHash = HashPassword(newPassword);
            admin.UpdatePassword(newPasswordHash);
            await _adminUserRepository.SaveAsync(admin);

            _logger.LogInformation("Password changed successfully for admin user: {AdminId}", adminId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for admin user: {AdminId}", adminId);
            throw;
        }
    }

    /// <summary>
    /// Generiert einen Passwort-Hash mit BCrypt
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Using BCrypt for password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Überprüft ein Passwort gegen einen BCrypt-Hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid arguments for password verification");
            return false;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid hash format for password verification");
            return false;
        }
    }
}
