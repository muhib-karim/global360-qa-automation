using Microsoft.Playwright;
using SnipeIt.UiTests.Components;
using SnipeIt.UiTests.Config;
using SnipeIt.UiTests.Data;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class AssetsPage(IPage page, SnipeSettings settings)
{
    private readonly BootstrapTable _table = new(page.Locator("#assetsListingTable"));

    public async Task SearchAsync(string tag)
    {
        await page.GotoAsync(new Uri(settings.BaseUrl, "/hardware").ToString());
        var search = page.GetByRole(AriaRole.Searchbox, new() { Name = "Search", Exact = true });
        await search.FillAsync(tag);
        await search.PressAsync("Enter");
    }

    public async Task VerifyRowAsync(AssetData asset, SelectedUser user)
    {
        var row = _table.RowsWith(asset.Tag);
        await Expect(row).ToHaveCountAsync(1);
        await Expect(row).ToContainTextAsync(asset.Name);
        await Expect(row).ToContainTextAsync(asset.Serial);
        await Expect(row).ToContainTextAsync(AssetData.Model);
        await Expect(row).ToContainTextAsync(AssetData.Status);
        await Expect(row.Locator($"a[href$='/users/{user.Id}']")).ToContainTextAsync(user.DisplayName);
    }

    public async Task OpenAsync(AssetData asset)
    {
        await _table.RowsWith(asset.Tag).GetByRole(AriaRole.Link, new() { Name = asset.Tag, Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading).Filter(new() { HasText = asset.Tag })).ToBeVisibleAsync();
    }
}
