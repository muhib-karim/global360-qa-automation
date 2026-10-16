using SnipeIt.UiTests.Accessibility;

namespace SnipeIt.UiTests.Tests.Unit;

[TestFixture]
[Category("Unit")]
public sealed class AxeAuditTests
{
    private static AxePageSummary Critical(params string[] rules) =>
        new(rules.Length, new Dictionary<string, int> { ["critical"] = rules.Length }, rules);

    [Test]
    public void Rules_in_the_baseline_are_not_new()
    {
        var found = new Dictionary<string, AxePageSummary> { ["login"] = Critical("label") };
        var baseline = new Dictionary<string, string[]> { ["login"] = ["label", "image-alt"] };

        Assert.That(AxeAudit.NewCriticalRules(found, baseline), Is.Empty);
    }

    [Test]
    public void New_rules_and_unknown_pages_are_reported()
    {
        var found = new Dictionary<string, AxePageSummary>
        {
            ["login"] = Critical("label", "button-name"),
            ["asset-page"] = Critical("image-alt"),
        };
        var baseline = new Dictionary<string, string[]> { ["login"] = ["label"] };

        var result = AxeAudit.NewCriticalRules(found, baseline);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result["login"], Has.Length.EqualTo(1).And.Contains("button-name"));
            Assert.That(result["asset-page"], Has.Length.EqualTo(1).And.Contains("image-alt"));
        }
    }
}
