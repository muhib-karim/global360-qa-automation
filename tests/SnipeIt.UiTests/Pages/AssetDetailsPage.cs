using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SnipeIt.UiTests.Components;
using SnipeIt.UiTests.Data;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed partial class AssetDetailsPage(IPage page)
{
    public async Task VerifyDetailsAsync(AssetData asset, SelectedUser user)
    {
        await VerifyValueAsync(".js-copy-asset_tag", asset.Tag);
        await VerifyValueAsync(".js-copy-asset_name", asset.Name);
        await VerifyValueAsync(".js-copy-license_key", asset.Serial);
        await VerifyValueAsync(".js-copy-asset_model", AssetData.Model);
        await VerifyValueAsync(".js-copy-notes", asset.Notes);
        var deployment = page.Locator("#details .well").Filter(new() { HasText = AssetData.Status });
        await Expect(deployment).ToContainTextAsync(AssetData.Status);
        await Expect(deployment).ToContainTextAsync("Deployed");
        await Expect(deployment.Locator($"a[href$='/users/{user.Id}']"))
            .ToHaveTextAsync(user.DisplayName);
    }

    public async Task OpenHistoryAsync()
    {
        await page.Locator("a[href='#history']").ClickAsync();
        await Expect(page.Locator("#history")).ToBeVisibleAsync();
    }

    public async Task VerifyHistoryAsync(AssetData asset, SelectedUser user, string actor)
    {
        var table = new BootstrapTable(page.Locator("#assetHistoryListingTable"));
        await Expect(table.Rows).ToHaveCountAsync(2);
        var created = RowWithAction(table, "create new");
        var checkout = RowWithAction(table, "checkout");
        await Expect(created).ToHaveCountAsync(1);
        await Expect(checkout).ToHaveCountAsync(1);
        var assetPath = new Uri(page.Url).AbsolutePath;
        foreach (var row in new[] { created, checkout })
        {
            var item = row.Locator($"a[href$='{assetPath}']");
            await Expect(item).ToContainTextAsync(asset.Tag);
            await Expect(item).ToContainTextAsync(asset.Name);
            await Expect(item).ToContainTextAsync(AssetData.Model);
            await Expect(await table.CellAsync(row, "created_by")).ToHaveTextAsync(actor);
            await Expect(await table.CellAsync(row, "created_at")).ToHaveTextAsync(HistoryDate());
            await Expect(await table.CellAsync(row, "quantity")).ToHaveTextAsync("1");
        }
        await Expect(await table.CellAsync(created, "target")).ToBeEmptyAsync();
        await Expect((await table.CellAsync(checkout, "target"))
            .Locator($"a[href$='/users/{user.Id}']")).ToHaveTextAsync(user.DisplayName);
        await Expect(await table.CellAsync(checkout, "note"))
            .ToHaveTextAsync("Checked out on asset creation");
    }

    private ILocator RowWithAction(BootstrapTable table, string action) =>
        table.Rows.Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = action, Exact = true }) });

    private async Task VerifyValueAsync(string selector, string expected)
    {
        var value = page.Locator($"#main {selector}");
        await Expect(value).ToBeVisibleAsync();
        await Expect(value).ToHaveTextAsync(expected);
    }

    // For example "Fri Oct 16, 2026 2:04AM" with the demo's default date settings.
    [GeneratedRegex(@"\w{3} \w{3} \d{1,2}, \d{4} \d{1,2}:\d{2}[AP]M")]
    private static partial Regex HistoryDate();
}
