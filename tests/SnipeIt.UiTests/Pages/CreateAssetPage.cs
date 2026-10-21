using Microsoft.Playwright;
using SnipeIt.UiTests.Components;
using SnipeIt.UiTests.Config;
using SnipeIt.UiTests.Data;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class CreateAssetPage(IPage page, SnipeSettings settings)
{
    public async Task OpenAsync()
    {
        await page.GotoAsync(new Uri(settings.BaseUrl, "/hardware/create").ToString());
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Assets Create New", Exact = true }))
            .ToBeVisibleAsync();
    }

    public async Task<SelectedUser> CreateAndCheckOutAsync(AssetData asset, Random random)
    {
        await page.GetByLabel("Asset Tag", new() { Exact = true }).FillAsync(asset.Tag);
        await page.Locator("#serial_1").FillAsync(asset.Serial);
        await new Select2(page, "model_select_id").SearchAndSelectAsync("Macbook Pro 13", AssetData.Model);
        await SelectDeployableStatusAsync();
        var users = new Select2(page, "assigned_user_select");
        var optionText = await users.SelectRandomAsync(random);
        var id = await users.SelectedOption.GetAttributeAsync("value")
                 ?? throw new InvalidOperationException("Selected user has no ID.");
        var user = new SelectedUser(id, optionText.Split(" (")[0], optionText);
        await page.Locator("#optional_info").ClickAsync();
        await page.Locator("#name").FillAsync(asset.Name);
        await page.Locator("#notes").FillAsync(asset.Notes);
        await SubmitAsync();
        return user;
    }

    /// <summary>Creates a Ready to Deploy asset that is not checked out to anyone.</summary>
    public async Task CreateAsync(AssetData asset)
    {
        await page.GetByLabel("Asset Tag", new() { Exact = true }).FillAsync(asset.Tag);
        await page.Locator("#serial_1").FillAsync(asset.Serial);
        await new Select2(page, "model_select_id").SearchAndSelectAsync("Macbook Pro 13", AssetData.Model);
        await SelectDeployableStatusAsync();
        await page.Locator("#optional_info").ClickAsync();
        await page.Locator("#name").FillAsync(asset.Name);
        await page.Locator("#notes").FillAsync(asset.Notes);
        await SubmitAsync();
    }

    // Picking a status asks the API whether it is deployable and then fades in this message and the "Checkout to"
    // section, which moves everything below it. Waiting for the message keeps the next click from landing on a
    // shifting layout (the nightly of 2026-10-21 lost its "optional information" click that way).
    private async Task SelectDeployableStatusAsync()
    {
        await new Select2(page, "status_select_id").SelectExactAsync(AssetData.Status);
        await Expect(page.Locator("#selected_status_status")).ToContainTextAsync("This asset can be checked out.");
    }

    // The demo redirects after a save and does not always render the success flash there (seen on WebKit), so the
    // outcome is verified in the list, on the asset page and through the API rather than on the flash.
    private async Task SubmitAsync()
    {
        var createUrl = new Uri(settings.BaseUrl, "/hardware/create").ToString();
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page).Not.ToHaveURLAsync(createUrl);
        await Expect(page.Locator(".alert-danger")).ToHaveCountAsync(0);
    }

    /// <summary>
    /// Submits the form with the asset tag cleared and no model or status. The form marks its required fields,
    /// refuses to submit, and the create page stays, so nothing is created on the demo.
    /// </summary>
    public async Task SubmitWithoutRequiredFieldsAsync()
    {
        var tag = page.GetByLabel("Asset Tag", new() { Exact = true });
        await tag.FillAsync("");
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Assets Create New", Exact = true }))
            .ToBeVisibleAsync();
        await Expect(page).ToHaveURLAsync(new Uri(settings.BaseUrl, "/hardware/create").ToString());
        await Expect(tag).ToHaveAttributeAsync("required", "");
        await Expect(page.Locator("#asset_tag:invalid")).ToHaveCountAsync(1);
        await Expect(page.Locator("#model_select_id:invalid")).ToHaveCountAsync(1);
        await Expect(page.Locator("#status_select_id:invalid")).ToHaveCountAsync(1);
        await Expect(page.Locator(".alert-success")).ToHaveCountAsync(0);
    }
}
