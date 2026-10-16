using SnipeIt.UiTests.Api;
using SnipeIt.UiTests.Data;
using SnipeIt.UiTests.Fixtures;
using SnipeIt.UiTests.Logging;
using SnipeIt.UiTests.Pages;

namespace SnipeIt.UiTests.Tests;

/// <summary>
/// The rest of an asset's life after the lifecycle test: check-in, edit, delete, and the form refusing an asset
/// without its required fields. Every test signs in fresh and uses its own unique asset.
/// </summary>
[TestFixture]
[Category("E2E")]
[Parallelizable(ParallelScope.Self)]
public sealed class AssetManagementTests : SnipeItTest
{
    [Test]
    public async Task Checked_out_asset_can_be_checked_in()
    {
        var seed = Settings.Snipe.UserSeed ?? Random.Shared.Next();
        var asset = new AssetDataBuilder().Build();
        var note = $"Checked in by the UI test {asset.Tag}";
        RunInfo["assetTag"] = asset.Tag;
        RunInfo["userSeed"] = seed;
        Logger.AssetPrepared(asset.Tag, asset.Name, asset.Serial, seed);

        var login = new LoginPage(Page, Settings.Snipe);
        await StepAsync("Open the login page", login.OpenAsync);
        var actor = await StepAsync("Sign in with the public demo admin", login.SignInAsync);
        RunInfo["actor"] = actor;

        var create = new CreateAssetPage(Page, Settings.Snipe);
        await StepAsync("Open the create asset form", create.OpenAsync);
        var user = await StepAsync("Create a Ready to Deploy Macbook Pro 13\" checked out to a random user",
            () => create.CreateAndCheckOutAsync(asset, new Random(seed)));
        RunInfo["user"] = user;
        Logger.UserSelected(user.DisplayName, user.Id, user.OptionText);

        var assets = new AssetsPage(Page, Settings.Snipe);
        await StepAsync("Search the assets list by the unique tag", () => assets.SearchAsync(asset.Tag));
        await StepAsync("Verify the asset is checked out", () => assets.VerifyRowAsync(asset, user));
        await StepAsync("Open the asset page from the list", () => assets.OpenAsync(asset));
        var assetUrl = new Uri(Page.Url);

        var details = new AssetDetailsPage(Page);
        await StepAsync("Check the asset in with a note", () => details.CheckInAsync(note));
        await EvidenceAsync("1-checked-in");
        await StepAsync("Reopen the asset page", () => details.OpenAsync(assetUrl, asset));
        await StepAsync("Verify the asset is no longer assigned", () => details.VerifyCheckedInAsync(asset));
        await EvidenceAsync("2-asset-after-checkin");
        await StepAsync("Open the History tab", details.OpenHistoryAsync);
        await StepAsync("Verify the check-in row", () => details.VerifyCheckInHistoryAsync(user, actor, note));
        await EvidenceAsync("3-asset-history");
        Logger.AssetVerified(Page.Url);

        var api = new SnipeItApi(Page, Settings.Snipe);
        var (status, found) = await StepAsync("Cross-check the asset through the REST API (read-only)",
            () => api.FindAssetsAsync(asset.Tag));
        Logger.ApiChecked(asset.Tag, status, found);
        RunInfo["apiStatus"] = status;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(status, Is.EqualTo(200), "GET /api/v1/hardware status");
            Assert.That(found, Is.EqualTo(new[] { new ApiAsset(asset.Tag, asset.Name, asset.Serial, AssetData.Model, AssetData.Status, null) }));
        }
    }

    [Test]
    public async Task Asset_can_be_edited_and_deleted()
    {
        var asset = new AssetDataBuilder().Build();
        var updated = asset with { Name = $"{asset.Name} renamed", Notes = $"{asset.Notes} (edited)" };
        RunInfo["assetTag"] = asset.Tag;

        var login = new LoginPage(Page, Settings.Snipe);
        await StepAsync("Open the login page", login.OpenAsync);
        RunInfo["actor"] = await StepAsync("Sign in with the public demo admin", login.SignInAsync);

        var create = new CreateAssetPage(Page, Settings.Snipe);
        await StepAsync("Open the create asset form", create.OpenAsync);
        await StepAsync("Create a Ready to Deploy Macbook Pro 13\" that is not checked out", () => create.CreateAsync(asset));

        var assets = new AssetsPage(Page, Settings.Snipe);
        await StepAsync("Search the assets list by the unique tag", () => assets.SearchAsync(asset.Tag));
        await StepAsync("Open the asset page from the list", () => assets.OpenAsync(asset));
        var assetUrl = new Uri(Page.Url);

        var details = new AssetDetailsPage(Page);
        await StepAsync("Edit the name and notes", () => details.EditAsync(updated));
        await StepAsync("Reopen the asset page", () => details.OpenAsync(assetUrl, updated));
        await StepAsync("Verify the edited values", () => details.VerifyEditedAsync(updated));
        await EvidenceAsync("1-asset-after-edit");

        var api = new SnipeItApi(Page, Settings.Snipe);
        var (status, found) = await StepAsync("Cross-check the edit through the REST API (read-only)",
            () => api.FindAssetsAsync(asset.Tag));
        Logger.ApiChecked(asset.Tag, status, found);
        RunInfo["apiStatus"] = status;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(status, Is.EqualTo(200), "GET /api/v1/hardware status after the edit");
            Assert.That(found, Is.EqualTo(new[] { new ApiAsset(asset.Tag, updated.Name, asset.Serial, AssetData.Model, AssetData.Status, null) }));
        }

        await StepAsync("Delete the asset and confirm the dialog", details.DeleteAsync);
        await EvidenceAsync("2-asset-deleted");
        await StepAsync("Search the assets list by the tag again", () => assets.SearchAsync(asset.Tag));
        await StepAsync("Verify the asset is gone from the list", () => assets.VerifyMissingAsync(asset));
        await EvidenceAsync("3-assets-list-after-delete");
        var (statusAfter, foundAfter) = await StepAsync("Cross-check the delete through the REST API (read-only)",
            () => api.FindAssetsAsync(asset.Tag));
        Logger.ApiChecked(asset.Tag, statusAfter, foundAfter);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(statusAfter, Is.EqualTo(200), "GET /api/v1/hardware status after the delete");
            Assert.That(foundAfter, Is.Empty, "a deleted asset must not be listed by the API");
        }
    }

    [Test]
    public async Task Asset_without_required_fields_is_refused()
    {
        var login = new LoginPage(Page, Settings.Snipe);
        await StepAsync("Open the login page", login.OpenAsync);
        RunInfo["actor"] = await StepAsync("Sign in with the public demo admin", login.SignInAsync);

        var create = new CreateAssetPage(Page, Settings.Snipe);
        await StepAsync("Open the create asset form", create.OpenAsync);
        await StepAsync("Submit the form with no asset tag, model or status", create.SubmitWithoutRequiredFieldsAsync);
        await EvidenceAsync("1-create-asset-refused");
    }
}
