using System.Globalization;
using Microsoft.Playwright;
using SnipeIt.UiTests.Components;
using SnipeIt.UiTests.Data;
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

    public async Task OpenHistoryAsync()
    {
        await page.Locator("a[href='#history']").ClickAsync();
        await Expect(page.Locator("#history")).ToBeVisibleAsync();
    }

    /// <summary>The two History rows of a new checked-out asset; their timestamps must belong to this run.</summary>
    public async Task VerifyHistoryAsync(AssetData asset, SelectedUser user, string actor, DateTime startedUtc)
    {
        var table = new BootstrapTable(page.Locator("#assetHistoryListingTable"));
        await Expect(table.Rows).ToHaveCountAsync(2);
        var created = RowWithAction(table, "create new");
        var checkout = RowWithAction(table, "checkout");
        await Expect(created).ToHaveCountAsync(1);
        await Expect(checkout).ToHaveCountAsync(1);
        var assetPath = new Uri(page.Url).AbsolutePath;
        var timestamps = new List<DateTime>();
        foreach (var row in new[] { created, checkout })
        {
            var item = row.Locator($"a[href$='{assetPath}']");
            await Expect(item).ToContainTextAsync(asset.Tag);
            await Expect(item).ToContainTextAsync(asset.Name);
            await Expect(item).ToContainTextAsync(AssetData.Model);
            await Expect(await table.CellAsync(row, "created_by")).ToHaveTextAsync(actor);
            timestamps.Add(await HistoryTimestampAsync(await table.CellAsync(row, "created_at"), startedUtc));
            await Expect(await table.CellAsync(row, "quantity")).ToHaveTextAsync("1");
        }
        await Expect(await table.CellAsync(created, "target")).ToBeEmptyAsync();
        await Expect((await table.CellAsync(checkout, "target"))
            .Locator($"a[href$='/users/{user.Id}']")).ToHaveTextAsync(user.DisplayName);
        await Expect(await table.CellAsync(checkout, "note"))
            .ToHaveTextAsync("Checked out on asset creation");
        // Both rows come from the one create-and-check-out submit.
        Assert.That((timestamps[1] - timestamps[0]).Duration(), Is.LessThanOrEqualTo(TimeSpan.FromMinutes(2)),
            "The create new and checkout rows should be logged together");
    }

    /// <summary>Opens the asset page by its URL (after a redirect to the listing) and waits for the tag heading.</summary>
    public async Task OpenAsync(Uri url, AssetData asset)
    {
        await page.GotoAsync(url.ToString());
        await Expect(page.GetByRole(AriaRole.Heading).Filter(new() { HasText = asset.Tag })).ToBeVisibleAsync();
    }

    /// <summary>Checks the asset in through its own Checkin button and form; the demo then redirects away.</summary>
    public async Task CheckInAsync(string note)
    {
        var assetUrl = new Uri(page.Url);
        await page.Locator($"a[href$='{assetUrl.AbsolutePath}/checkin']").ClickAsync();
        await Expect(page).ToHaveURLAsync(new Uri(assetUrl, $"{assetUrl.AbsolutePath}/checkin").ToString());
        await Expect(page.Locator("#submit_button")).ToBeVisibleAsync();
        await page.Locator("#note").FillAsync(note);
        await page.Locator("#submit_button").ClickAsync();
        // The demo redirects to the listing; the success flash is not always rendered there, so the outcome is
        // verified on the asset page and through the API rather than on the flash.
        await Expect(page).Not.ToHaveURLAsync(new Uri(assetUrl, $"{assetUrl.AbsolutePath}/checkin").ToString());
        await Expect(page.Locator(".alert-danger")).ToHaveCountAsync(0);
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
        var assetUrl = new Uri(page.Url);
        var editUrl = new Uri(assetUrl, $"{assetUrl.AbsolutePath}/edit").ToString();
        await page.Locator($"a[href$='{assetUrl.AbsolutePath}/edit']").ClickAsync();
        await Expect(page).ToHaveURLAsync(editUrl);
        await Expect(page.GetByLabel("Asset Tag", new() { Exact = true })).ToHaveValueAsync(updated.Tag);
        await new OptionalInfoSection(page).OpenAsync();
        await page.Locator("#name").FillAsync(updated.Name);
        await page.Locator("#notes").FillAsync(updated.Notes);
        await page.Locator("#submit_button").ClickAsync();
        // Same as the check-in: the save redirects away and the flash is not reliable, the asset page is.
        await Expect(page).Not.ToHaveURLAsync(editUrl);
        await Expect(page.Locator(".alert-danger")).ToHaveCountAsync(0);
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
        var assetUrl = page.Url;
        await dialog.Locator("#dataConfirmOK").ClickAsync();
        // The delete redirects to the listing; the list and the API prove the delete, not the flash.
        await Expect(page).Not.ToHaveURLAsync(assetUrl);
        await Expect(page.Locator(".alert-danger")).ToHaveCountAsync(0);
    }

    private ILocator RowWithAction(BootstrapTable table, string action) =>
        table.Rows.Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = action, Exact = true }) });

    private async Task VerifyValueAsync(string selector, string expected)
    {
        var value = page.Locator($"#main {selector}");
        await Expect(value).ToBeVisibleAsync();
        await Expect(value).ToHaveTextAsync(expected);
    }

    // The demo's date and time format is a shared setting that anyone can change ("Wed Oct 21, 2026 4:31AM",
    // "10/16/2026 07:20", ...), and its server timezone is not the runner's. So the cell is parsed with Snipe-IT's
    // own formats and must be a server-local time of this run in some real timezone.
    private static async Task<DateTime> HistoryTimestampAsync(ILocator cell, DateTime startedUtc)
    {
        await Expect(cell).Not.ToBeEmptyAsync();
        var text = await cell.InnerTextAsync();
        var readings = SnipeItDate.Parse(text)
            .Where(reading => SnipeItDate.IsPlausible(reading, startedUtc, DateTime.UtcNow))
            .ToList();
        var started = startedUtc.ToString("u", CultureInfo.InvariantCulture);
        Assert.That(readings, Is.Not.Empty,
            $"History created_at '{text}' should be a Snipe-IT date and time from this run (started {started})");
        return readings[0];
    }
}
