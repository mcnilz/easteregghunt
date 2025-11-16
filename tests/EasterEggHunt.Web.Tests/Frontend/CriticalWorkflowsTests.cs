using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend;

/// <summary>
/// Historische Sammelklasse für Playwright-Workflows (migriert).
/// Beibehalten, aber als ignoriert markiert, da die Tests in Admin/Employee aufgeteilt wurden.
/// </summary>
[TestFixture]
[Category("Playwright")]
[Ignore("Tests wurden in feature-basierte Klassen unter Admin/ und Employee/ migriert.")]
public sealed class CriticalWorkflowsTests
{
}

