using System.Globalization;
using System.Text.RegularExpressions;

namespace SnipeIt.UiTests.Data;

/// <summary>
/// Reads the date and time Snipe-IT displays in its tables. The display format is an admin setting (a date format
/// plus a time format, shared by everyone on the demo), so every combination Snipe-IT offers is accepted, and only
/// those. Parsing uses the invariant culture: the demo renders English month names and Gregorian years whatever
/// the culture of the machine running the tests.
/// </summary>
internal static partial class SnipeItDate
{
    // Snipe-IT's date_display_format options, as .NET patterns: Y-m-d, D M d Y, M j Y, d M Y, m/d/Y, n/d/y,
    // d/m/Y, d.m.Y, Y.m.d.
    private static readonly string[] Dates =
    [
        "yyyy-MM-dd", "ddd MMM dd, yyyy", "MMM d, yyyy", "dd MMM, yyyy", "MM/dd/yyyy", "M/dd/yy", "dd/MM/yyyy",
        "dd.MM.yyyy", "yyyy.MM.dd.",
    ];

    // time_display_format options: g:iA (3:14PM), h:i A (03:14 PM), H:i (15:14).
    private static readonly string[] Times = ["h:mmtt", "hh:mm tt", "HH:mm"];

    /// <summary>Every reading of <paramref name="text"/>; two when day and month order cannot be told apart.</summary>
    public static IReadOnlyList<DateTime> Parse(string text)
    {
        var normalized = Whitespace().Replace(text, " ").Trim();
        return Dates.SelectMany(date => Times.Select(time => $"{date} {time}"))
            .Select(format => DateTime.TryParseExact(normalized, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var value) ? value : (DateTime?)null)
            .OfType<DateTime>()
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// True if <paramref name="display"/> is a server-local time of a moment between <paramref name="startedUtc"/>
    /// and <paramref name="nowUtc"/> in some real timezone (UTC-12 to UTC+14). Snipe-IT shows whole minutes, so
    /// the display can be up to a minute earlier than the moment.
    /// </summary>
    public static bool IsPlausible(DateTime display, DateTime startedUtc, DateTime nowUtc) =>
        display >= startedUtc.AddHours(-12).AddMinutes(-1) && display <= nowUtc.AddHours(14);

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
