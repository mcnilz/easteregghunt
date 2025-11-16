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
    /// Erstellt eine neue Campaign
    /// </summary>
    public async Task CreateCampaignAsync(string name, string description)
    {
        await NavigateToCreateAsync();
        await FillCreateFormAsync(name, description);
        // Klicke den Submit-Button des Formulars auf der CreateCampaign-Seite explizit
        await _page.ClickAsync("form[data-loading='true'] button[type='submit']");

        // Warte auf Redirect zur Campaigns-Liste oder auf Fehler
        await Task.WhenAny(
            _page.WaitForURLAsync("**/Admin/Campaigns**", new PageWaitForURLOptions { Timeout = 10000 }),
            _page.WaitForSelectorAsync(".text-danger", new PageWaitForSelectorOptions { Timeout = 5000 })
        );
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

