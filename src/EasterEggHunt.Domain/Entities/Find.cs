namespace EasterEggHunt.Domain.Entities;

/// <summary>
/// Repräsentiert einen Fund eines QR-Codes durch einen Benutzer
/// </summary>
public class Find
{
    /// <summary>
    /// Eindeutige ID des Fundes
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID des gefundenen QR-Codes
    /// </summary>
    public int QrCodeId { get; set; }

    /// <summary>
    /// ID des Benutzers, der den QR-Code gefunden hat
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Zeitpunkt des Fundes
    /// </summary>
    public DateTime FoundAt { get; set; }

    /// <summary>
    /// IP-Adresse des Benutzers beim Fund
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User-Agent des Browsers beim Fund
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Navigation Property zum gefundenen QR-Code
    /// </summary>
    public virtual QrCode QrCode { get; set; } = null!;

    /// <summary>
    /// Navigation Property zum Benutzer
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Konstruktor für Entity Framework
    /// </summary>
    public Find()
    {
    }

    /// <summary>
    /// Erstellt einen neuen Fund
    /// </summary>
    /// <param name="qrCodeId">ID des gefundenen QR-Codes</param>
    /// <param name="userId">ID des Benutzers</param>
    /// <param name="ipAddress">IP-Adresse des Benutzers</param>
    /// <param name="userAgent">User-Agent des Browsers</param>
    public Find(int qrCodeId, int userId, string ipAddress, string userAgent)
    {
        QrCodeId = qrCodeId;
        UserId = userId;
        IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        UserAgent = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
        FoundAt = DateTime.UtcNow;
    }
}


