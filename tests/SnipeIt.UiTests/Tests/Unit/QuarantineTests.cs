using System.Globalization;
using System.Reflection;
using SnipeIt.UiTests.Fixtures;

namespace SnipeIt.UiTests.Tests.Unit;

[TestFixture]
[Category("Unit")]
public sealed class QuarantineTests
{
    [Test]
    public void A_quarantined_test_carries_the_category_the_gating_run_excludes()
    {
        Assert.That(new QuarantineAttribute("Flaky on webkit, issue #1", "2026-11-01"),
            Is.InstanceOf<CategoryAttribute>().With.Property(nameof(CategoryAttribute.Name)).EqualTo("Quarantine"));
    }

    [Test]
    public void Every_quarantine_names_its_reason_and_has_not_expired()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var quarantines = typeof(QuarantineTests).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods().Cast<MemberInfo>().Prepend(type))
            .SelectMany(member => member.GetCustomAttributes<QuarantineAttribute>()
                .Select(quarantine => (member.Name, quarantine.Reason, quarantine.Until)));

        using (Assert.EnterMultipleScope())
        {
            foreach (var (name, reason, until) in quarantines)
            {
                Assert.That(reason, Is.Not.Empty, $"{name}: the reason should name the tracking issue");
                Assert.That(DateOnly.ParseExact(until, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Is.GreaterThanOrEqualTo(today), $"{name}: the quarantine expired; fix the test or delete it");
            }
        }
    }
}
