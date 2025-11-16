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
}


