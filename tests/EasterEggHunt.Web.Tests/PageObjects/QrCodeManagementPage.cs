using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die QR-Code-Management-Seite
/// </summary>
public class QrCodeManagementPage
{
    private readonly IPage _page;

    public QrCodeManagementPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigiert zur QR-Codes-Übersicht einer Campaign
    /// </summary>
    public async Task NavigateAsync(int campaignId)
    {
        await _page.GotoAsync($"/Admin/QrCodes/{campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zur QR-Code-Erstellung
    /// </summary>
    public async Task NavigateToCreateAsync(int campaignId)
    {
        await _page.GotoAsync($"/Admin/CreateQrCode?campaignId={campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Füllt das QR-Code-Erstellungsformular aus
    /// </summary>
    public async Task FillCreateFormAsync(string title, string description, string internalNotes = "")
    {
        await _page.FillAsync("input[name='Title']", title);
        if (!string.IsNullOrEmpty(description))
        {
            await _page.FillAsync("textarea[name='Description']", description);
        }
        // InternalNotes ist im Model Required – fülle einen Standardwert, wenn leer
        var notes = string.IsNullOrWhiteSpace(internalNotes) ? "Automated Test" : internalNotes;
        await _page.FillAsync("textarea[name='InternalNotes']", notes);
    }

    /// <summary>
    /// Erstellt einen neuen QR-Code
    /// </summary>
    public async Task CreateQrCodeAsync(int campaignId, string title, string description, string internalNotes = "")
    {
        await NavigateToCreateAsync(campaignId);
        await FillCreateFormAsync(title, description, internalNotes);

        // Klicke den Submit-Button (spezifisches Formular)
        await _page.ClickAsync("form[data-loading='true'] button[type='submit']");

        // Warte auf Redirect zur QR-Codes-Liste oder auf Fehler
        await Task.WhenAny(
            _page.WaitForURLAsync($"**/Admin/QrCodes/{campaignId}**", new PageWaitForURLOptions { Timeout = 10000 }),
            _page.WaitForSelectorAsync(".text-danger", new PageWaitForSelectorOptions { Timeout = 5000 })
        );
    }

    /// <summary>
    /// Prüft, ob ein QR-Code in der Liste vorhanden ist
    /// </summary>
    public async Task<bool> HasQrCodeAsync(string qrCodeTitle)
    {
        try
        {
            var qrCodeElement = await _page.QuerySelectorAsync($"text={qrCodeTitle}");
            return qrCodeElement != null;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    /// <summary>
    /// Prüft, ob eine Success-Message angezeigt wird
    /// </summary>
    public async Task<bool> HasSuccessMessageAsync()
    {
        try
        {
            var successElement = await _page.QuerySelectorAsync(".alert-success");
            return successElement != null;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    /// <summary>
    /// Klickt auf einen QR-Code in der Liste (öffnet Details)
    /// </summary>
    public async Task ClickQrCodeAsync(string qrCodeTitle)
    {
        await _page.ClickAsync($"text={qrCodeTitle}");
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Klickt in der Tabelle in der Zeile des angegebenen QR-Codes auf den Bearbeiten-Button
    /// </summary>
    public async Task ClickEditForAsync(string qrCodeTitle)
    {
        // Finde die Tabellenzeile mit dem Titel und klicke dort auf "Bearbeiten"
        var row = _page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = qrCodeTitle });
        await row.Locator("text=Bearbeiten").ClickAsync();
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Klickt in der Tabelle in der Zeile des angegebenen QR-Codes auf den Löschen-Button
    /// </summary>
    public async Task ClickDeleteForAsync(string qrCodeTitle)
    {
        var row = _page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = qrCodeTitle });
        await row.Locator("text=Löschen").ClickAsync();
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Füllt das Edit-Formular mit neuen Werten
    /// </summary>
    public async Task FillEditFormAsync(string newTitle, string newDescription, string newInternalNotes)
    {
        await _page.FillAsync("input[name='Title']", newTitle);
        await _page.FillAsync("textarea[name='Description']", newDescription);
        await _page.FillAsync("textarea[name='InternalNotes']", string.IsNullOrWhiteSpace(newInternalNotes) ? "Updated by Automated Test" : newInternalNotes);
    }

    /// <summary>
    /// Sendet das Edit-Formular ab und wartet auf Rückkehr zur Liste
    /// </summary>
    public async Task SubmitEditAsync(int campaignId)
    {
        await _page.ClickAsync("form[data-loading='true'] button[type='submit']");
        await _page.WaitForURLAsync($"**/Admin/QrCodes/{campaignId}**", new PageWaitForURLOptions { Timeout = 10000 });
    }

    /// <summary>
    /// Bestätigt die Löschung in der Bestätigungsansicht und wartet auf Rückkehr zur Liste
    /// </summary>
    public async Task ConfirmDeleteAsync(int campaignId)
    {
        await _page.ClickAsync("form[action*='DeleteQrCode'] button[type='submit']");
        await _page.WaitForURLAsync($"**/Admin/QrCodes/{campaignId}**", new PageWaitForURLOptions { Timeout = 10000 });
    }

    /// <summary>
    /// Öffnet die Druckansicht über den "Drucken"-Button auf der QR-Liste der Kampagne
    /// </summary>
    public async Task OpenPrintLayoutAsync(int campaignId)
    {
        // Sicherstellen, dass wir uns auf der Liste befinden
        // Notwendig, falls der Aufrufer nicht vorher NavigateAsync(campaignId) genutzt hat
        if (!_page.Url.Contains($"/Admin/QrCodes/{campaignId}", StringComparison.OrdinalIgnoreCase))
        {
            await NavigateAsync(campaignId);
        }

        // Klicke den "Drucken"-Button in der Toolbar und warte auf die Druckansicht
        await _page.GetByRole(AriaRole.Link, new() { Name = "Drucken" }).ClickAsync();
        await _page.WaitForURLAsync($"**/Admin/PrintQrCodes/{campaignId}**", new PageWaitForURLOptions { Timeout = 20000 });
        await _page.WaitForSelectorAsync("h1, .print-header", new PageWaitForSelectorOptions { Timeout = 20000 });
    }
}


