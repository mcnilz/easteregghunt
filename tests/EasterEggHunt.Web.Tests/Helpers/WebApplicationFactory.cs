using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// WebApplicationFactory für Integration-Tests des Web-Projekts
/// Überschreibt CreateHost, um Kestrel statt TestServer zu verwenden (für Playwright)
/// </summary>
public class WebApplicationFactory : WebApplicationFactory<IWebMarker>
{
    private Uri? _realServerUrl;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        // Verwende einen zufälligen Port für Kestrel
        builder.UseUrls("http://localhost:0");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Erstelle einen echten Kestrel-Server statt TestServer
        // Dies ermöglicht Playwright, über HTTP auf den Server zuzugreifen

        // Konfiguriere den Host-Builder für Kestrel
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseKestrel();
            webBuilder.UseUrls("http://localhost:5000");
        });

        var testHost = builder.Build();
        testHost.Start();

        // Warte kurz, damit der Server vollständig gestartet ist
        Thread.Sleep(100);

        // Extrahiere die tatsächliche URL des Servers
        var server = testHost.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        var urlString = addresses?.Addresses.FirstOrDefault();

        if (urlString == null)
        {
            throw new InvalidOperationException("Server-Adresse konnte nicht ermittelt werden!");
        }

        _realServerUrl = new Uri(urlString);

        return testHost;
    }

    public Uri ServerUrl
    {
        get => _realServerUrl ?? throw new InvalidOperationException("Server-URL wurde nicht initialisiert!");
    }
}

