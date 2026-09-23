using System.Globalization;

namespace SanStudioZoomAutomation.Configuration;

// Converts settings values to and from the text shown in the UI.
public static class SettingsText
{
    private static readonly string[] ClockFormats = ["h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt", "H:mm", "HH:mm"];
    private static readonly string[] DurationFormats = [@"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss"];

    public static string FormatClock(TimeSpan time)
    {
        var minuteOfDay = (int)(((long)time.TotalMinutes % 1440 + 1440) % 1440);
        return DateTime.MinValue.AddMinutes(minuteOfDay).ToString("hh:mm tt", CultureInfo.InvariantCulture);
    }

    public static bool TryParseClock(string? text, out TimeSpan time)
    {
        time = default;
        if (!DateTime.TryParseExact(text?.Trim(), ClockFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return false;

        time = parsed.TimeOfDay;
        return true;
    }

    public static string FormatDuration(TimeSpan duration)
    {
        var format = duration.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss";
        return duration.ToString(format, CultureInfo.InvariantCulture);
    }

    public static bool TryParseDuration(string? text, out TimeSpan duration)
    {
        return TimeSpan.TryParseExact(text?.Trim(), DurationFormats, CultureInfo.InvariantCulture, out duration);
    }

    // Contiguous runs of three or more days read as a range, e.g. "Monday – Saturday".
    public static string FormatDays(IEnumerable<DayOfWeek> days)
    {
        var chosen = days.ToHashSet();
        var ordered = ApplicationSettings.WeekOrder.Where(chosen.Contains).ToList();
        if (ordered.Count == 0)
            return "No days selected";

        var first = Array.IndexOf(ApplicationSettings.WeekOrder, ordered[0]);
        var last = Array.IndexOf(ApplicationSettings.WeekOrder, ordered[^1]);
        var contiguous = last - first + 1 == ordered.Count;

        return contiguous && ordered.Count >= 3
            ? $"{ordered[0]} – {ordered[^1]}"
            : string.Join(", ", ordered);
    }
}
