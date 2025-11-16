using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// Helper-Klasse für Admin-Login-Operationen in Playwright-Tests
/// </summary>
public static class LoginHelper
{
    /// <summary>
    /// Standard-Admin-Credentials
    /// </summary>
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "admin123";

    /// <summary>
    /// Führt einen Admin-Login durch
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <param name="username">Benutzername (Standard: admin)</param>
    /// <param name="password">Passwort (Standard: admin123)</param>
    /// <returns>Task</returns>
    public static async Task LoginAsAdminAsync(IPage page, string username = DefaultAdminUsername, string password = DefaultAdminPassword)
    {
        // Navigiere zur Login-Seite
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Login-Formular
        await page.WaitForSelectorAsync("input[name='Username']", new PageWaitForSelectorOptions { Timeout = 10000 });

        // Fülle Login-Formular aus
        await page.FillAsync("input[name='Username']", username);
        await page.FillAsync("input[name='Password']", password);

        // Optional: "Remember Me" Checkbox setzen
        // await page.CheckAsync("input[name='RememberMe']");

        // Submit-Button klicken
        await page.ClickAsync("button[type='submit']");

        // Warte auf Redirect zur Admin-Seite oder auf Login-Fehler
        await Task.WhenAny(
            page.WaitForURLAsync("**/Admin/**", new PageWaitForURLOptions { Timeout = 10000 }),
            page.WaitForSelectorAsync(".alert-danger", new PageWaitForSelectorOptions { Timeout = 5000 })
        );
    }

    /// <summary>
    /// Prüft, ob der Benutzer eingeloggt ist (durch Prüfung der URL oder Navigation)
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <returns>True wenn eingeloggt, sonst False</returns>
    public static bool IsLoggedIn(IPage page)
    {
        var url = page.Url;
        return url.Contains("/Admin/", StringComparison.OrdinalIgnoreCase) ||
               url.Contains("/Auth/Login", StringComparison.OrdinalIgnoreCase) == false;
    }

    /// <summary>
    /// Führt einen Logout durch
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <returns>Task</returns>
    public static async Task LogoutAsync(IPage page)
    {
        // Navigiere zur Logout-Seite oder klicke auf Logout-Link
        await page.GotoAsync("/Auth/Logout", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf Redirect zur Login-Seite
        await page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 10000 });
    }
}

