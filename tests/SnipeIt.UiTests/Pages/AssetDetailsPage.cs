using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SnipeIt.UiTests.Support;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.UiTests.Pages;

internal sealed class AssetDetailsPage(IPage page)
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

    public async Task VerifyHistoryAsync(AssetData asset, SelectedUser user, string actor)
    {
        await page.Locator("a[href='#history']").ClickAsync();
        await Expect(page.Locator("#history")).ToBeVisibleAsync();
        var table = page.Locator("#assetHistoryListingTable");
        var rows = table.Locator("tbody tr");
        await Expect(rows).ToHaveCountAsync(2);
        var created = rows.Filter(new()
        {
            Has = page.GetByRole(AriaRole.Cell,
            new() { Name = "create new", Exact = true })
        });
        var checkout = rows.Filter(new()
        {
            Has = page.GetByRole(AriaRole.Cell,
            new() { Name = "checkout", Exact = true })
        });
        await Expect(created).ToHaveCountAsync(1);
        await Expect(checkout).ToHaveCountAsync(1);
        var assetPath = new Uri(page.Url).AbsolutePath;
        foreach (var row in new[] { created, checkout })
        {
            var item = row.Locator($"a[href$='{assetPath}']");
            await Expect(item).ToContainTextAsync(asset.Tag);
            await Expect(item).ToContainTextAsync(asset.Name);
            await Expect(item).ToContainTextAsync(AssetData.Model);
            await Expect(await CellAsync(table, row, "created_by")).ToHaveTextAsync(actor);
            await Expect(await CellAsync(table, row, "created_at")).ToHaveTextAsync(
                new Regex(@"\w{3} \w{3} \d{1,2}, \d{4} \d{1,2}:\d{2}[AP]M"));
            await Expect(await CellAsync(table, row, "quantity")).ToHaveTextAsync("1");
        }
        await Expect(await CellAsync(table, created, "target")).ToBeEmptyAsync();
        await Expect((await CellAsync(table, checkout, "target"))
            .Locator($"a[href$='/users/{user.Id}']")).ToHaveTextAsync(user.DisplayName);
        await Expect(await CellAsync(table, checkout, "note"))
            .ToHaveTextAsync("Checked out on asset creation");
    }

    private async Task VerifyValueAsync(string selector, string expected)
    {
        var value = page.Locator($"#main {selector}");
        await Expect(value).ToBeVisibleAsync();
        await Expect(value).ToHaveTextAsync(expected);
    }

    private static async Task<ILocator> CellAsync(ILocator table, ILocator row, string field)
    {
        var fields = await table.Locator("thead th").EvaluateAllAsync<string[]>(
            "headers => headers.map(h => h.getAttribute('data-field'))");
        var column = Array.IndexOf(fields, field);
        if (column < 0)
            throw new InvalidOperationException($"History column '{field}' is missing.");
        return row.Locator("td").Nth(column);
    }
}
