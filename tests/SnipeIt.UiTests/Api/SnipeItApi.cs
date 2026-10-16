using System.Net;
using System.Text.Json;
using Microsoft.Playwright;
using SnipeIt.UiTests.Config;

namespace SnipeIt.UiTests.Api;

internal sealed record ApiAsset(string Tag, string Name, string Serial, string Model, string Status, string? AssignedUserId);

/// <summary>
/// Read-only Snipe-IT REST calls made with the signed-in browser session (the same cookie and CSRF token the UI's
/// own tables use). The demo disables personal access tokens ("This feature has been disabled for the demo
/// installation" on /account/api), and a token would be a write to the shared admin account anyway.
/// </summary>
internal sealed class SnipeItApi(IPage page, SnipeSettings settings)
{

    public async Task<(int Status, ApiAsset[] Assets)> FindAssetsAsync(string search)
    {
        var token = await page.Locator("meta[name='csrf-token']").GetAttributeAsync("content") ?? "";
        var url = new Uri(settings.BaseUrl, $"/api/v1/hardware?search={Uri.EscapeDataString(search)}");
        var response = await page.APIRequest.GetAsync(url.ToString(), new()
        {
            Headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["X-Requested-With"] = "XMLHttpRequest",
                ["X-CSRF-TOKEN"] = token,
            },
        });
        if (!response.Ok)
            return (response.Status, []);
        var json = await response.JsonAsync() ?? throw new InvalidOperationException("Empty API response.");
        var assets = json.GetProperty("rows").EnumerateArray().Select(row => new ApiAsset(
            Text(row, "asset_tag"),
            Text(row, "name"),
            Text(row, "serial"),
            Text(row.GetProperty("model"), "name"),
            Text(row.GetProperty("status_label"), "name"),
            row.GetProperty("assigned_to") is { ValueKind: JsonValueKind.Object } assigned
                ? assigned.GetProperty("id").ToString()
                : null)).ToArray();
        return (response.Status, assets);
    }

    // The API HTML-encodes text fields, for example Macbook Pro 13&quot;
    private static string Text(JsonElement element, string property) =>
        WebUtility.HtmlDecode(element.GetProperty(property).GetString() ?? "");
}
