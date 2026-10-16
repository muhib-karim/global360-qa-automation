using System.Net;
using System.Text.Json;
using Microsoft.Playwright;
using SnipeIt.UiTests.Config;

namespace SnipeIt.UiTests.Api;

internal sealed record ApiAsset(string Tag, string Name, string Serial, string Model, string Status, string? AssignedUserId);

/// <summary>
/// Read-only Snipe-IT REST calls made with the signed-in browser session (the same cookie and CSRF token the UI's
/// own tables use), so no personal access token is created on the shared demo.
/// </summary>
internal sealed class SnipeItApi(IPage page, SnipeSettings settings)
{
    private static readonly string[] TokenPageHints = ["disabled", "demo", "Create New Token", "personal access"];

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

    // shortcut: temporary probe for the README's personal access token question, removed after one CI round.
    public async Task<string> ProbeTokenPageAsync()
    {
        var response = await page.APIRequest.GetAsync(new Uri(settings.BaseUrl, "/account/api").ToString());
        var body = await response.TextAsync();
        var hints = TokenPageHints.Where(h => body.Contains(h, StringComparison.OrdinalIgnoreCase));
        return $"GET /account/api -> {response.Status} {response.Url}; hints: {string.Join(", ", hints)}";
    }

    // The API HTML-encodes text fields, for example Macbook Pro 13&quot;
    private static string Text(JsonElement element, string property) =>
        WebUtility.HtmlDecode(element.GetProperty(property).GetString() ?? "");
}
