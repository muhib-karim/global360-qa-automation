using Microsoft.Playwright;
using SnipeIt.UiTests.Support;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class CreateAssetPage(IPage page, TestSettings settings)
{
    public async Task OpenAsync()
    {
        await page.GotoAsync(new Uri(settings.BaseUrl, "/hardware/create").ToString());
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Assets Create New", Exact = true }))
            .ToBeVisibleAsync();
    }

    public async Task<SelectedUser> CreateAndCheckOutAsync(AssetData asset)
    {
        await page.GetByLabel("Asset Tag", new() { Exact = true }).FillAsync(asset.Tag);
        await page.Locator("#serial_1").FillAsync(asset.Serial);
        await new Select2(page, "model_select_id").SearchAndSelectAsync("Macbook Pro 13", AssetData.Model);
        await new Select2(page, "status_select_id").SelectExactAsync(AssetData.Status);
        var optionText = await new Select2(page, "assigned_user_select")
            .SelectRandomAsync(new Random(settings.UserSeed));
        var selected = page.Locator("#assigned_user_select option:checked");
        var id = await selected.GetAttributeAsync("value")
                 ?? throw new InvalidOperationException("Selected user has no ID.");
        var displayName = optionText.Split(" (")[0];
        var user = new SelectedUser(id, displayName, optionText);
        await page.Locator("#optional_info").ClickAsync();
        await page.Locator("#name").FillAsync(asset.Name);
        await page.Locator("#notes").FillAsync(asset.Notes);
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page.Locator(".alert-success")).ToContainTextAsync("Success");
        return user;
    }
}
