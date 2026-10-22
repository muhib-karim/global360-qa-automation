using SnipeIt.UiTests.Data;

namespace SnipeIt.UiTests.Tests.Unit;

[TestFixture]
[Category("Unit")]
public sealed class SnipeItDateTests
{
    // One case per Snipe-IT date display format, each with one of its three time display formats.
    [TestCase("Wed Oct 21, 2026 4:31AM", 2026, 10, 21, 4, 31)] // D M d, Y g:iA (the nightly of 2026-10-21)
    [TestCase("10/16/2026 07:20", 2026, 10, 16, 7, 20)] // m/d/Y H:i
    [TestCase("2026-10-21 04:31 PM", 2026, 10, 21, 16, 31)] // Y-m-d h:i A
    [TestCase("Oct 5, 2026 12:05PM", 2026, 10, 5, 12, 5)] // M j, Y g:iA
    [TestCase("05 Oct, 2026 16:31", 2026, 10, 5, 16, 31)] // d M, Y H:i
    [TestCase("10/21/26 4:31AM", 2026, 10, 21, 4, 31)] // n/d/y g:iA
    [TestCase("21/10/2026 16:31", 2026, 10, 21, 16, 31)] // d/m/Y H:i
    [TestCase("21.10.2026 04:31 AM", 2026, 10, 21, 4, 31)] // d.m.Y h:i A
    [TestCase("2026.10.21. 16:31", 2026, 10, 21, 16, 31)] // Y.m.d. H:i
    [TestCase("  Wed Oct 21, 2026 4:31AM ", 2026, 10, 21, 4, 31)] // cell padding and a non-breaking space
    public void Reads_every_Snipe_IT_display_format(string text, int year, int month, int day, int hour, int minute)
    {
        Assert.That(SnipeItDate.Parse(text), Is.EqualTo(new[] { new DateTime(year, month, day, hour, minute, 0) }));
    }

    [Test]
    public void Day_and_month_that_cannot_be_told_apart_give_both_readings()
    {
        Assert.That(SnipeItDate.Parse("03/04/2026 10:00"),
            Is.EquivalentTo(new[] { new DateTime(2026, 3, 4, 10, 0, 0), new DateTime(2026, 4, 3, 10, 0, 0) }));
    }

    [TestCase("Thu Oct 21, 2026 4:31AM")] // the weekday does not match the date
    [TestCase("2026-10-21")] // no time
    [TestCase("4:31AM")] // no date
    [TestCase("Wed Oct 21, 2026 4:31")] // g:iA without its AM or PM
    [TestCase("13/13/2026 10:00")]
    [TestCase("yesterday")]
    [TestCase("")]
    public void Rejects_text_that_is_not_a_full_date_and_time(string text)
    {
        Assert.That(SnipeItDate.Parse(text), Is.Empty);
    }

    [Test]
    [SetCulture("th-TH")] // Buddhist calendar, Thai month names and a different AM/PM designator
    public void Reading_does_not_depend_on_the_runner_culture()
    {
        Assert.That(SnipeItDate.Parse("Wed Oct 21, 2026 4:31AM"), Is.EqualTo(new[] { new DateTime(2026, 10, 21, 4, 31, 0) }));
    }

    private static readonly DateTime StartedUtc = new(2026, 10, 21, 8, 31, 7, DateTimeKind.Utc);
    private static readonly DateTime NowUtc = StartedUtc.AddSeconds(20);

    // The demo showed 4:31AM for an asset created at 08:31 UTC: its server runs at UTC-4.
    [TestCase(-12 * 60)]
    [TestCase(-4 * 60)]
    [TestCase(0)]
    [TestCase(14 * 60)]
    public void A_display_time_from_the_run_in_any_server_timezone_is_plausible(int offsetMinutes)
    {
        var display = StartedUtc.AddMinutes(offsetMinutes);
        display = new DateTime(display.Year, display.Month, display.Day, display.Hour, display.Minute, 0);

        Assert.That(SnipeItDate.IsPlausible(display, StartedUtc, NowUtc), Is.True);
    }

    [TestCase(-12 * 60 - 2)]
    [TestCase(14 * 60 + 2)]
    [TestCase(-24 * 60)] // a row from the day before
    public void A_display_time_no_server_timezone_explains_is_not(int offsetMinutes)
    {
        Assert.That(SnipeItDate.IsPlausible(StartedUtc.AddMinutes(offsetMinutes), StartedUtc, NowUtc), Is.False);
    }
}
