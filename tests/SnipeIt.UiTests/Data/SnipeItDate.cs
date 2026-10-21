namespace SnipeIt.UiTests.Data;

/// <summary>Reads the date and time Snipe-IT displays in its tables.</summary>
internal static class SnipeItDate
{
    public static IReadOnlyList<DateTime> Parse(string text) => [];

    public static bool IsPlausible(DateTime display, DateTime startedUtc, DateTime nowUtc) => false;
}
