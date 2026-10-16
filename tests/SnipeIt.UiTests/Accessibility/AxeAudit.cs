using System.Text.Json;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using SnipeIt.UiTests.Logging;

namespace SnipeIt.UiTests.Accessibility;

internal sealed record AxePageSummary(int Violations, IReadOnlyDictionary<string, int> RulesByImpact, string[] CriticalRules);

/// <summary>axe-core WCAG 2.x A/AA scan of a page. Results are recorded; the gate compares critical rules with a baseline.</summary>
internal sealed class AxeAudit(IPage page, string outputDir, ILogger logger)
{
    private static readonly AxeRunOptions Options = new()
    {
        RunOnly = new RunOnlyOptions { Type = "tag", Values = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"] },
    };

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public Dictionary<string, AxePageSummary> Summary { get; } = [];

    public async Task ScanAsync(string pageName)
    {
        var result = await page.RunAxe(Options);
        var violations = result.Violations.Select(v => new
        {
            v.Id,
            v.Impact,
            v.Help,
            v.HelpUrl,
            Nodes = v.Nodes.Select(n => n.Html).ToArray(),
        }).ToArray();
        await File.WriteAllTextAsync(Path.Combine(outputDir, $"axe-{pageName}.json"),
            JsonSerializer.Serialize(new { Page = pageName, page.Url, Violations = violations }, Json));
        var critical = violations.Where(v => v.Impact == "critical").Select(v => v.Id).Order(StringComparer.Ordinal).ToArray();
        var byImpact = violations.GroupBy(v => v.Impact ?? "unknown")
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        Summary[pageName] = new AxePageSummary(violations.Length, byImpact, critical);
        logger.AxeScanned(pageName, violations.Length, critical);
    }

    /// <returns>Per page, the critical rules that are not in the baseline. Empty when nothing new appeared.</returns>
    public static Dictionary<string, string[]> NewCriticalRules(
        IReadOnlyDictionary<string, AxePageSummary> found, IReadOnlyDictionary<string, string[]> baseline) =>
        found.Select(p => (p.Key, New: p.Value.CriticalRules.Except(baseline.GetValueOrDefault(p.Key, []), StringComparer.Ordinal).ToArray()))
            .Where(p => p.New.Length > 0)
            .ToDictionary(p => p.Key, p => p.New, StringComparer.Ordinal);

    public static Dictionary<string, string[]> LoadBaseline(string path) =>
        JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(path)) ?? [];
}
