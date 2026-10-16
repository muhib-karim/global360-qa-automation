using SnipeIt.UiTests.Accessibility;
using SnipeIt.UiTests.Api;
using SnipeIt.UiTests.Data;
using SnipeIt.UiTests.Fixtures;
using SnipeIt.UiTests.Logging;
using SnipeIt.UiTests.Pages;

namespace SnipeIt.UiTests.Tests;

[TestFixture]
[Category("E2E")]
[Parallelizable(ParallelScope.Self)]
public sealed class AssetLifecycleTests : SnipeItTest
{
    [Test]
    public async Task Macbook_can_be_created_checked_out_found_and_audited()
    {
        var seed = Settings.Snipe.UserSeed ?? Random.Shared.Next();
        var asset = new AssetDataBuilder().Build();
        RunInfo["assetTag"] = asset.Tag;
        RunInfo["userSeed"] = seed;
        Logger.AssetPrepared(asset.Tag, asset.Name, asset.Serial, seed);
        var audit = new AxeAudit(Page, ArtifactsDir, Logger);

        var login = new LoginPage(Page, Settings.Snipe);
        await StepAsync("Open the login page", login.OpenAsync);
        await audit.ScanAsync("login");
        var actor = await StepAsync("Sign in with the public demo admin", login.SignInAsync);
        RunInfo["actor"] = actor;

        var create = new CreateAssetPage(Page, Settings.Snipe);
        await StepAsync("Open the create asset form", create.OpenAsync);
        await audit.ScanAsync("create-asset");
        var user = await StepAsync("Create a Ready to Deploy Macbook Pro 13\" checked out to a random user",
            () => create.CreateAndCheckOutAsync(asset, new Random(seed)));
        RunInfo["user"] = user;
        Logger.UserSelected(user.DisplayName, user.Id, user.OptionText);

        var assets = new AssetsPage(Page, Settings.Snipe);
        await StepAsync("Search the assets list by the unique tag", () => assets.SearchAsync(asset.Tag));
        await StepAsync("Verify the asset row", () => assets.VerifyRowAsync(asset, user));
        await audit.ScanAsync("assets-list");
        await EvidenceAsync("1-assets-list");
        await StepAsync("Open the asset page from the list", () => assets.OpenAsync(asset));

        var details = new AssetDetailsPage(Page);
        await StepAsync("Verify the asset details", () => details.VerifyDetailsAsync(asset, user));
        await audit.ScanAsync("asset-page");
        await EvidenceAsync("2-asset-details");
        await StepAsync("Open the History tab", details.OpenHistoryAsync);
        await StepAsync("Verify the History rows", () => details.VerifyHistoryAsync(asset, user, actor));
        await EvidenceAsync("3-asset-history");
        Logger.AssetVerified(Page.Url);

        var api = new SnipeItApi(Page, Settings.Snipe);
        var probe = await api.ProbeTokenPageAsync();
        Logger.ApiProbed(probe);
        var (status, found) = await StepAsync("Cross-check the asset through the REST API (read-only)",
            () => api.FindAssetsAsync(asset.Tag));
        Logger.ApiChecked(asset.Tag, status, found);
        RunInfo["apiStatus"] = status;

        RunInfo["accessibility"] = audit.Summary;
        var newCritical = AxeAudit.NewCriticalRules(audit.Summary,
            AxeAudit.LoadBaseline(Path.Combine(AppContext.BaseDirectory, "accessibility-baseline.json")));
        RunInfo["accessibilityNewCritical"] = newCritical;
        if (Settings.Accessibility.Enforce)
            Assert.That(newCritical, Is.Empty, "Critical accessibility rules not in accessibility-baseline.json");
    }
}
