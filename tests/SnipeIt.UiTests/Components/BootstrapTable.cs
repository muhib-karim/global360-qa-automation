using Microsoft.Playwright;

namespace SnipeIt.UiTests.Components;

/// <summary>Snipe-IT's bootstrap-table listings. Columns are found by their data-field header, never by position.</summary>
internal sealed class BootstrapTable(ILocator table)
{
    public ILocator Rows => table.Locator("tbody tr");

    public ILocator RowsWith(string text) => Rows.Filter(new() { HasText = text });

    public async Task<ILocator> CellAsync(ILocator row, string field)
    {
        var fields = await table.Locator("thead th").EvaluateAllAsync<string?[]>(
            "headers => headers.map(h => h.getAttribute('data-field'))");
        var column = Array.IndexOf(fields, field);
        if (column < 0)
            throw new InvalidOperationException($"Column '{field}' is missing.");
        return row.Locator("td").Nth(column);
    }
}
