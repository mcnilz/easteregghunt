using System.ComponentModel.DataAnnotations;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für das Login-Formular
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Benutzername des Administrators
    /// </summary>
    [Required(ErrorMessage = "Benutzername ist erforderlich")]
    [StringLength(50, ErrorMessage = "Benutzername darf maximal 50 Zeichen haben")]
    [Display(Name = "Benutzername")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Passwort des Administrators
    /// </summary>
    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [StringLength(100, ErrorMessage = "Passwort darf maximal 100 Zeichen haben")]
    [Display(Name = "Passwort")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ob die Session länger gespeichert werden soll
    /// </summary>
    [Display(Name = "Angemeldet bleiben")]
    public bool RememberMe { get; set; }

    /// <summary>
    /// URL zur Weiterleitung nach erfolgreichem Login
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "URL wird als String für Redirect verwendet")]
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// ViewModel für Access Denied Seite
/// </summary>
public class AccessDeniedViewModel
{
    /// <summary>
    /// Angeforderte URL
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "URL wird als String für Anzeige verwendet")]
    public string? RequestedUrl { get; set; }

    /// <summary>
    /// Benutzerfreundliche Fehlermeldung
    /// </summary>
    public string Message { get; set; } = "Sie haben keine Berechtigung, auf diese Seite zuzugreifen.";
}
