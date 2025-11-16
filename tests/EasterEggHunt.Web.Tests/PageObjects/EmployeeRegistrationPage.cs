using System.Diagnostics.CodeAnalysis;
using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die Employee-Registrierungsseite
/// </summary>
public class EmployeeRegistrationPage
{
    private readonly IPage _page;

    public EmployeeRegistrationPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigiert zur Registrierungsseite mit QR-Code-URL
    /// </summary>
    [SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = "QR-Code URL is a relative path segment, not a full URI")]
    public async Task NavigateAsync(string qrCodeUrl)
    {
        var url = $"/Employee/Register?qrCodeUrl={Uri.EscapeDataString(qrCodeUrl)}";
        await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Füllt das Registrierungsformular aus
    /// </summary>
    public async Task FillRegistrationFormAsync(string name)
    {
        await _page.FillAsync("input[name='Name']", name);
    }

    /// <summary>
    /// Registriert einen neuen Employee
    /// </summary>
    public async Task RegisterAsync(string name)
    {
        await FillRegistrationFormAsync(name);

        // Klicke den Submit-Button des spezifischen Formulars
        await _page.ClickAsync("form[data-loading='true'] button[type='submit']");

        // Warte auf Redirect zur QR-Code-Scan-Seite oder auf echte Fehlermeldung
        // Hinweis: .text-danger ist zu generisch (leere Validierungs-Container existieren immer)
        await Task.WhenAny(
            _page.WaitForURLAsync("**/qr/**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync(".alert-danger, .validation-summary-errors", new PageWaitForSelectorOptions { Timeout = 7000 })
        );
    }

    /// <summary>
    /// Prüft, ob eine Fehlermeldung angezeigt wird
    /// </summary>
    public async Task<bool> HasErrorMessageAsync()
    {
        try
        {
            var errorElement = await _page.QuerySelectorAsync(".alert-danger");
            return errorElement != null;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    /// <summary>
    /// Holt die Fehlermeldung (falls vorhanden)
    /// </summary>
    public async Task<string?> GetErrorMessageAsync()
    {
        try
        {
            var errorElement = await _page.QuerySelectorAsync(".alert-danger");
            if (errorElement != null)
            {
                return await errorElement.TextContentAsync();
            }
        }
        catch (PlaywrightException)
        {
            // Ignoriere Fehler beim Abrufen der Fehlermeldung
        }
        return null;
    }

    /// <summary>
    /// Prüft, ob das Formular sichtbar ist
    /// </summary>
    public async Task<bool> IsFormVisibleAsync()
    {
        try
        {
            var formElement = await _page.QuerySelectorAsync("form[data-loading='true']");
            return formElement != null;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }
}

