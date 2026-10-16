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
        await new Select2(page, "status_select_id").SelectExactAsync(AssetData.Status);
        var users = new Select2(page, "assigned_user_select");
        var optionText = await users.SelectRandomAsync(random);
        var id = await users.SelectedOption.GetAttributeAsync("value")
                 ?? throw new InvalidOperationException("Selected user has no ID.");
        var user = new SelectedUser(id, optionText.Split(" (")[0], optionText);
        await page.Locator("#optional_info").ClickAsync();
        await page.Locator("#name").FillAsync(asset.Name);
        await page.Locator("#notes").FillAsync(asset.Notes);
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page.Locator(".alert-success")).ToContainTextAsync("Success");
        return user;
    }

    /// <summary>Creates a Ready to Deploy asset that is not checked out to anyone.</summary>
    public async Task CreateAsync(AssetData asset)
    {
        await page.GetByLabel("Asset Tag", new() { Exact = true }).FillAsync(asset.Tag);
        await page.Locator("#serial_1").FillAsync(asset.Serial);
        await new Select2(page, "model_select_id").SearchAndSelectAsync("Macbook Pro 13", AssetData.Model);
        await new Select2(page, "status_select_id").SelectExactAsync(AssetData.Status);
        await page.Locator("#optional_info").ClickAsync();
        await page.Locator("#name").FillAsync(asset.Name);
        await page.Locator("#notes").FillAsync(asset.Notes);
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page.Locator(".alert-success")).ToContainTextAsync("Success");
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
