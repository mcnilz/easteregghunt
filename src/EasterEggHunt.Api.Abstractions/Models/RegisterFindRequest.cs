namespace EasterEggHunterApi.Abstractions.Models;

/// <summary>
/// Request-Model für Fund-Registrierung
/// </summary>
public class RegisterFindRequest
{
    /// <summary>
    /// QR-Code-ID
    /// </summary>
    public int QrCodeId { get; set; }

    /// <summary>
    /// Benutzer-ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// IP-Adresse
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User-Agent
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
}
