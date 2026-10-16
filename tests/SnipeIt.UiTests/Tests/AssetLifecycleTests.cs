using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using SnipeIt.UiTests.Pages;
using SnipeIt.UiTests.Support;

namespace SnipeIt.UiTests.Tests;

[TestFixture]
[NonParallelizable]
public sealed class AssetLifecycleTests : PageTest
{
    private string _artifacts = null!;

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        ViewportSize = new() { Width = 1440, Height = 1000 },
        Locale = "en-US"
    };

    [SetUp]
    public async Task StartDiagnostics()
    {
        Page.SetDefaultTimeout(20_000);
        Assertions.SetDefaultExpectTimeout(20_000);
        _artifacts = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.WorkDirectory,
            "artifacts", $"{DateTime.UtcNow:yyyyMMddTHHmmss}-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(_artifacts);
        await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
    }

    [Test]
    public async Task Macbook_can_be_created_checked_out_found_and_audited()
    {
        var settings = TestSettings.FromEnvironment();
        var asset = AssetData.Unique();
        TestContext.Progress.WriteLine($"Asset tag: {asset.Tag}; random user seed: {settings.UserSeed}");
        var actor = await new LoginPage(Page, settings).SignInAsync();
        var create = new CreateAssetPage(Page, settings);
        await create.OpenAsync();
        var user = await create.CreateAndCheckOutAsync(asset);
        TestContext.Progress.WriteLine($"Selected demo user: {user.OptionText}; ID: {user.Id}");
        await new AssetsPage(Page, settings).FindAndOpenAsync(asset, user);
        var details = new AssetDetailsPage(Page);
        await details.VerifyDetailsAsync(asset, user);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(_artifacts, "asset-details.png"), FullPage = true });
        await details.VerifyHistoryAsync(asset, user, actor);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(_artifacts, "asset-history.png"), FullPage = true });
        await File.WriteAllTextAsync(Path.Combine(_artifacts, "run.json"),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                asset.Tag,
                asset.Name,
                asset.Serial,
                asset.Notes,
                Model = AssetData.Model,
                Status = AssetData.Status,
                User = user,
                Actor = actor,
                settings.UserSeed,
                Url = Page.Url,
                VerifiedAtUtc = DateTime.UtcNow
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        TestContext.Progress.WriteLine($"Verified asset: {Page.Url}");
    }

    [TearDown]
    public async Task SaveDiagnostics()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            await File.WriteAllTextAsync(Path.Combine(_artifacts, "failure.html"), await Page.ContentAsync());
            await Page.ScreenshotAsync(new() { Path = Path.Combine(_artifacts, "failure.png"), FullPage = true });
        }
        var trace = Path.Combine(_artifacts, "trace.zip");
        await Context.Tracing.StopAsync(new() { Path = trace });
        TestContext.AddTestAttachment(trace, "Playwright trace");
        TestContext.Progress.WriteLine($"Artifacts: {_artifacts}");
    }
}
