using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die Admin-Login-Seite
/// </summary>
public class AdminLoginPage
{
    private readonly IPage _page;

    public AdminLoginPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigiert zur Login-Seite
    /// </summary>
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.WaitForSelectorAsync("input[name='Username']");
    }

    /// <summary>
    /// Füllt das Login-Formular aus
    /// </summary>
    public async Task FillLoginFormAsync(string username, string password, bool rememberMe = false)
    {
        await _page.FillAsync("input[name='Username']", username);
        await _page.FillAsync("input[name='Password']", password);

        if (rememberMe)
        {
            await _page.CheckAsync("input[name='RememberMe']");
        }
    }

    /// <summary>
    /// Klickt auf den Submit-Button
    /// </summary>
    public async Task SubmitAsync()
    {
        await _page.ClickAsync("button[type='submit']");
    }

    /// <summary>
    /// Führt einen vollständigen Login durch
    /// </summary>
    public async Task LoginAsync(string username, string password, bool rememberMe = false)
    {
        await NavigateAsync();
        await FillLoginFormAsync(username, password, rememberMe);

        // Speichere die aktuelle URL vor dem Submit
        var currentUrl = _page.Url;

        // Submit-Button klicken und auf Navigation warten
        await SubmitAsync();

        // Warte auf URL-Änderung oder Fehlermeldung
        // Verwende Task.WhenAny, um auf beide Möglichkeiten zu warten
        var adminRedirectTask = _page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 10000 });
        var errorMessageTask = _page.WaitForSelectorAsync(".alert-danger", new PageWaitForSelectorOptions { Timeout = 10000 });

        var completedTask = await Task.WhenAny(adminRedirectTask, errorMessageTask);

        // Wenn die URL sich nicht geändert hat, warte kurz und prüfe erneut
        if (_page.Url == currentUrl || _page.Url.Contains("/Auth/Login", StringComparison.OrdinalIgnoreCase))
        {
            // Warte auf vollständiges Laden der Seite
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Prüfe erneut, ob wir auf einer Admin-Seite sind
            if (!_page.Url.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Wenn wir noch auf der Login-Seite sind, prüfe ob ein Fehler angezeigt wird
                var hasError = await HasErrorMessageAsync();
                if (!hasError)
                {
                    // Wenn kein Fehler angezeigt wird, warte noch etwas länger auf die Weiterleitung
                    try
                    {
                        await _page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 5000 });
                    }
                    catch (TimeoutException)
                    {
                        // Timeout - Login war möglicherweise nicht erfolgreich
                    }
                }
            }
        }
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
    /// Gibt die Fehlermeldung zurück (falls vorhanden)
    /// </summary>
    public async Task<string?> GetErrorMessageAsync()
    {
        try
        {
            var errorElement = await _page.QuerySelectorAsync(".alert-danger");
            return errorElement != null ? await errorElement.TextContentAsync() : null;
        }
        catch (PlaywrightException)
        {
            return null;
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
}

