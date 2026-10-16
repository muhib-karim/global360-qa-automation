using Microsoft.Playwright;
using SnipeIt.UiTests.Support;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class AssetsPage(IPage page, TestSettings settings)
{
    public async Task FindAndOpenAsync(AssetData asset, SelectedUser user)
    {
        await page.GotoAsync(new Uri(settings.BaseUrl, "/hardware").ToString());
        var search = page.GetByRole(AriaRole.Searchbox, new() { Name = "Search", Exact = true });
        await search.FillAsync(asset.Tag);
        await search.PressAsync("Enter");
        var row = page.Locator("#assetsListingTable tbody tr").Filter(new() { HasText = asset.Tag });
        await Expect(row).ToHaveCountAsync(1);
        await Expect(row).ToContainTextAsync(asset.Name);
        await Expect(row).ToContainTextAsync(asset.Serial);
        await Expect(row).ToContainTextAsync(AssetData.Model);
        await Expect(row).ToContainTextAsync(AssetData.Status);
        await Expect(row.Locator($"a[href$='/users/{user.Id}']")).ToContainTextAsync(user.DisplayName);
        await row.GetByRole(AriaRole.Link, new() { Name = asset.Tag, Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading).Filter(new() { HasText = asset.Tag })).ToBeVisibleAsync();
    }
}
