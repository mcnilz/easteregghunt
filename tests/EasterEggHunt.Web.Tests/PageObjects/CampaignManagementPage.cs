using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die Campaign-Management-Seite
/// </summary>
public class CampaignManagementPage
{
    private readonly IPage _page;

    public CampaignManagementPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigiert zur Campaigns-Übersicht
    /// </summary>
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/Admin/Campaigns", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zur Campaign-Erstellung
    /// </summary>
    public async Task NavigateToCreateAsync()
    {
        await _page.GotoAsync("/Admin/CreateCampaign", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await _page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Füllt das Campaign-Erstellungsformular aus
    /// </summary>
    public async Task FillCreateFormAsync(string name, string description)
    {
        await _page.FillAsync("input[name='Name']", name);
        await _page.FillAsync("textarea[name='Description']", description);
    }

    /// <summary>
    /// Erstellt eine neue Campaign und gibt die Campaign-ID zurück
    /// </summary>
    public async Task<int> CreateCampaignAsync(string name, string description)
    {
        await NavigateToCreateAsync();
        await FillCreateFormAsync(name, description);
        // Klicke den Submit-Button des Formulars auf der CreateCampaign-Seite explizit
        await _page.ClickAsync("form[data-loading='true'] button[type='submit']");

        // Warte auf Redirect zur Campaigns-Liste oder auf Fehler
        var redirectedTask = _page.WaitForURLAsync("**/Admin/Campaigns**", new PageWaitForURLOptions { Timeout = 15000 });
        var errorTask = _page.WaitForSelectorAsync(".text-danger", new PageWaitForSelectorOptions { Timeout = 7000 });
        var completed = await Task.WhenAny(redirectedTask, errorTask);
        if (completed == redirectedTask)
        {
            // Stelle sicher, dass die Seite vollständig geladen ist
            await _page.WaitForLoadStateAsync();
        }

        // Hole Campaign-ID aus der API
        return await GetCampaignIdFromApiAsync(name);
    }

    /// <summary>
    /// Ermittelt die Campaign-ID aus dem DOM der Kampagnenliste.
    /// Erwartet, dass die Tabelle Zeilen mit data-campaign-id trägt und der Kampagnenname im selben <tr> vorhanden ist.
    /// </summary>
    private async Task<int> GetCampaignIdFromApiAsync(string campaignName)
    {
        // Statt einen API-Call vom Browser auszuführen (der scheitern kann, z.B. wegen Auth),
        // lesen wir die ID direkt aus dem DOM der Kampagnenliste.
        try
        {
            var campaignId = await _page.EvaluateAsync<int?>(@"(name) => {
                const rows = Array.from(document.querySelectorAll('table tbody tr'));
                for (const row of rows) {
                    const containsName = row.textContent && row.textContent.trim().includes(name);
                    if (containsName) {
                        const idAttr = row.getAttribute('data-campaign-id');
                        const id = idAttr ? parseInt(idAttr, 10) : NaN;
                        if (!Number.isNaN(id)) {
                            return id;
                        }
                    }
                }
                return null;
            }", campaignName);

            if (campaignId.HasValue)
            {
                return campaignId.Value;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Konnte Campaign-ID für '{campaignName}' nicht aus dem DOM ermitteln: {ex.Message}");
        }

        throw new InvalidOperationException($"Konnte Campaign-ID für '{campaignName}' nicht finden. Ist die Kampagne in der Liste vorhanden?");
    }

    /// <summary>
    /// Prüft, ob eine Campaign in der Liste vorhanden ist
    /// </summary>
    public async Task<bool> HasCampaignAsync(string campaignName)
    {
        try
        {
            var campaignElement = await _page.QuerySelectorAsync($"text={campaignName}");
            return campaignElement != null;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    /// <summary>
    /// Klickt auf eine Campaign in der Liste
    /// </summary>
    public async Task ClickCampaignAsync(string campaignName)
    {
        await _page.ClickAsync($"text={campaignName}");
        await _page.WaitForLoadStateAsync();
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
}

