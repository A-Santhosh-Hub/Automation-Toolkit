using System.IO;

namespace SanStudioZoomAutomation.Configuration;

public static class SettingsValidator
{
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    // Returns one message per problem; an empty list means the settings are valid.
    public static List<string> Validate(ApplicationSettings settings)
    {
        var errors = new List<string>();

        if (settings.AutomationDays.Count == 0)
            errors.Add("Select at least one automation day.");

        var startValid = IsTimeOfDay(settings.MeetingStartTime);
        var endValid = IsTimeOfDay(settings.ExpectedEndTime);

        if (!startValid)
            errors.Add("Meeting Start Time must be a valid time of day.");
        if (!endValid)
            errors.Add("Expected End Time must be a valid time of day.");
        if (startValid && endValid && settings.ExpectedEndTime <= settings.MeetingStartTime)
            errors.Add("Expected End Time must be later than the Meeting Start Time.");

        if (settings.PreparationMinutes < ApplicationSettings.MinPreparationMinutes || settings.PreparationMinutes > ApplicationSettings.MaxPreparationMinutes)
            errors.Add($"Preparation Time must be between {ApplicationSettings.MinPreparationMinutes} and {ApplicationSettings.MaxPreparationMinutes} minutes.");

        ValidateZoomPath(settings.ZoomExecutablePath, errors);

        if (settings.NumberOfSystems < ApplicationSettings.MinSystems || settings.NumberOfSystems > ApplicationSettings.MaxSystems)
        {
            errors.Add($"Number of Systems must be between {ApplicationSettings.MinSystems} and {ApplicationSettings.MaxSystems}.");
            return errors;
        }

        var count = Math.Min(settings.NumberOfSystems, settings.Systems.Count);
        for (var i = 0; i < count; i++)
            ValidateSystem(settings.Systems[i], $"System {i + 1}", errors);

        return errors;
    }

    // Empty is fine: it means Zoom is found automatically. Anything else must be an existing .exe file.
    private static void ValidateZoomPath(string? path, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var trimmed = path.Trim();
        if (!trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            errors.Add($"Zoom Executable must be an .exe file, or left empty to detect Zoom automatically: {trimmed}");
        else if (!File.Exists(trimmed))
            errors.Add($"Zoom Executable file was not found: {trimmed}");
    }

    private static void ValidateSystem(SystemConfiguration system, string label, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(system.SystemName))
            errors.Add($"{label}: System Name cannot be empty.");

        ValidateDuration(system.IntroVideoDuration, $"{label}: Intro Video Duration", errors);
        ValidateDuration(system.PrayerVideoDuration, $"{label}: Prayer Video Duration", errors);

        // Video files are only checked for systems that are enabled.
        if (!system.Enabled)
            return;

        ValidateVideo(system.IntroVideoPath, system.IntroVideoDuration, $"{label}: Intro Video", errors);
        ValidateVideo(system.PrayerVideoPath, system.PrayerVideoDuration, $"{label}: Prayer Video", errors);
    }

    private static void ValidateDuration(TimeSpan duration, string field, List<string> errors)
    {
        if (duration < TimeSpan.Zero || duration >= OneDay)
            errors.Add($"{field} is not valid.");
    }

    private static void ValidateVideo(string path, TimeSpan duration, string field, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (!File.Exists(path))
            errors.Add($"{field} file was not found: {path}");
        else if (duration <= TimeSpan.Zero)
            errors.Add($"{field} Duration must be greater than 00:00 when a video is selected.");
    }

    private static bool IsTimeOfDay(TimeSpan time) => time >= TimeSpan.Zero && time < OneDay;
}
