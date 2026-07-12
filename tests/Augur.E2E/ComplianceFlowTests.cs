using System.Globalization;
using Microsoft.Playwright;
using Xunit;

namespace Augur.E2E;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:5000";

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null) await Browser.DisposeAsync();
    }

    public async Task<IPage> NewPageAsync(string route = "/")
    {
        var page = await Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl + route);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return page;
    }
}

[CollectionDefinition("Playwright")]
public sealed class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>;

[Collection("Playwright")]
public sealed class ComplianceFlowTests : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IBrowserContext _context = null!;

    public ComplianceFlowTests(PlaywrightFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _context = await _fixture.Browser.NewContextAsync();

    public async Task DisposeAsync()
    {
        if (_context is not null) await _context.DisposeAsync();
    }

    [Fact]
    public async Task Dashboard_Shows_Compliance_Coverage()
    {
        var page = await _context.NewPageAsync();
        await page.GotoAsync(PlaywrightFixture.BaseUrl + "/");
        await page.WaitForSelectorAsync("[data-testid='dashboard-header']");

        var total = await page.Locator("[data-testid='stat-total'] .grc-stat__value").InnerTextAsync();
        Assert.NotEqual("0", total.Trim());
    }

    [Fact]
    public async Task CreateControl_Adds_Row_To_ControlLibrary()
    {
        var page = await _context.NewPageAsync();
        await page.GotoAsync(PlaywrightFixture.BaseUrl + "/controls");
        await page.WaitForSelectorAsync("[data-testid='btn-create-control']");

        var code = "SOC2-E2E-" + Guid.NewGuid().ToString("N")[..6];
        await page.Locator("[data-testid='input-code']").FillAsync(code);
        await page.Locator("[data-testid='input-title']").FillAsync("E2E Control");
        await page.Locator("[data-testid='input-owner']").FillAsync("QA");
        await page.Locator("[data-testid='btn-create-control']").ClickAsync();

        // The new row should appear with the generated code.
        await page.WaitForSelectorAsync($"[data-testid='control-row-{code}']", new() { Timeout = 10000 });
        Assert.NotNull(await page.Locator($"[data-testid='control-row-{code}']").First.ElementHandleAsync());
    }

    [Fact]
    public async Task EvidenceUpload_Opens_Modal_And_Attaches()
    {
        var page = await _context.NewPageAsync();
        await page.GotoAsync(PlaywrightFixture.BaseUrl + "/evidence");
        await page.WaitForSelectorAsync("[data-testid='evidence-controls']");

        // Open the upload modal for the first control.
        await page.Locator("[data-testid^='btn-upload-']").First.ClickAsync();
        await page.WaitForSelectorAsync("[data-testid='evidence-modal']");
        await page.Locator("[data-testid='input-filename']").FillAsync("e2e-policy.pdf");
        await page.Locator("[data-testid='input-uploader']").FillAsync("qa-bot");
        await page.Locator("[data-testid='btn-submit-evidence']").ClickAsync();

        // Modal should close after submit.
        await page.WaitForSelectorAsync("[data-testid='evidence-modal']", new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    [Fact]
    public async Task RunAgent_Maps_Findings_And_Shows_Summary()
    {
        var page = await _context.NewPageAsync();
        await page.GotoAsync(PlaywrightFixture.BaseUrl + "/agent");
        await page.WaitForSelectorAsync("[data-testid='btn-run-agent']");

        await page.Locator("[data-testid='input-source']").FillAsync("e2e-scanner");
        await page.Locator("[data-testid='btn-run-agent']").ClickAsync();

        await page.WaitForSelectorAsync("[data-testid='agent-result']", new() { Timeout = 15000 });
        var summary = await page.Locator("[data-testid='agent-summary']").InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(summary));
    }
}

