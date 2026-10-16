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

    /// <summary>Opens the asset page by its URL (after a redirect to the listing) and waits for the tag heading.</summary>
    public async Task OpenAsync(Uri url, AssetData asset)
    {
        await page.GotoAsync(url.ToString());
        await Expect(page.GetByRole(AriaRole.Heading).Filter(new() { HasText = asset.Tag })).ToBeVisibleAsync();
    }

    /// <summary>Checks the asset in through its own Checkin button and form.</summary>
    public async Task CheckInAsync(string note)
    {
        var assetUrl = new Uri(page.Url);
        await page.Locator($"a[href$='{assetUrl.AbsolutePath}/checkin']").ClickAsync();
        await Expect(page).ToHaveURLAsync(new Uri(assetUrl, $"{assetUrl.AbsolutePath}/checkin").ToString());
        await Expect(page.Locator("#submit_button")).ToBeVisibleAsync();
        await page.Locator("#note").FillAsync(note);
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page.Locator(".alert-success")).ToContainTextAsync("checked in");
    }

    /// <summary>After a check-in: same asset, deployable status, nobody assigned.</summary>
    public async Task VerifyCheckedInAsync(AssetData asset)
    {
        await VerifyValueAsync(".js-copy-asset_tag", asset.Tag);
        await VerifyValueAsync(".js-copy-asset_name", asset.Name);
        var deployment = page.Locator("#details .well").Filter(new() { HasText = AssetData.Status });
        await Expect(deployment).ToContainTextAsync(AssetData.Status);
        await Expect(deployment).Not.ToContainTextAsync("Deployed");
        await Expect(deployment.Locator("a[href*='/users/']")).ToHaveCountAsync(0);
    }

    /// <summary>The History tab after a check-in: the two creation rows plus one "checkin from" row for the user.</summary>
    public async Task VerifyCheckInHistoryAsync(SelectedUser user, string actor, string note)
    {
        var table = new BootstrapTable(page.Locator("#assetHistoryListingTable"));
        await Expect(table.Rows).ToHaveCountAsync(3);
        var checkin = RowWithAction(table, "checkin from");
        await Expect(checkin).ToHaveCountAsync(1);
        await Expect(await table.CellAsync(checkin, "created_by")).ToHaveTextAsync(actor);
        await Expect((await table.CellAsync(checkin, "target")).Locator($"a[href$='/users/{user.Id}']"))
            .ToHaveTextAsync(user.DisplayName);
        await Expect(await table.CellAsync(checkin, "note")).ToHaveTextAsync(note);
    }

    /// <summary>Edits the name and notes through the asset's own Update button and form.</summary>
    public async Task EditAsync(AssetData updated)
    {
        var assetPath = new Uri(page.Url).AbsolutePath;
        await page.Locator($"a[href$='{assetPath}/edit']").ClickAsync();
        await Expect(page.GetByLabel("Asset Tag", new() { Exact = true })).ToHaveValueAsync(updated.Tag);
        await page.Locator("#optional_info").ClickAsync();
        await page.Locator("#name").FillAsync(updated.Name);
        await page.Locator("#notes").FillAsync(updated.Notes);
        await page.Locator("#submit_button").ClickAsync();
        await Expect(page.Locator(".alert-success")).ToContainTextAsync("updated");
    }

    /// <summary>After an edit: the new name and notes, and the untouched tag, serial and model.</summary>
    public async Task VerifyEditedAsync(AssetData updated)
    {
        await VerifyValueAsync(".js-copy-asset_tag", updated.Tag);
        await VerifyValueAsync(".js-copy-asset_name", updated.Name);
        await VerifyValueAsync(".js-copy-license_key", updated.Serial);
        await VerifyValueAsync(".js-copy-asset_model", AssetData.Model);
        await VerifyValueAsync(".js-copy-notes", updated.Notes);
    }

    /// <summary>Deletes the asset through its Delete button and the confirmation dialog.</summary>
    public async Task DeleteAsync()
    {
        await page.Locator("button.delete-asset").ClickAsync();
        var dialog = page.Locator("#dataConfirmModal");
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.Locator("#dataConfirmOK").ClickAsync();
        await Expect(page.Locator(".alert-success")).ToContainTextAsync("deleted");
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
