using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SanStudioZoomAutomation.Configuration;

// Loads, saves and provides default settings. Storage is a single JSON file.
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    public SettingsService(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SanStudio", "ZoomAutomation", "settings.json");
    }

    public string FilePath { get; }

    // Set by Load() when the file exists but could not be used; null otherwise.
    public string? LoadWarning { get; private set; }

    public static ApplicationSettings CreateDefaults()
    {
        var settings = new ApplicationSettings();
        for (var number = 1; number <= ApplicationSettings.MaxSystems; number++)
            settings.Systems.Add(CreateDefaultSystem(number));

        return settings;
    }

    public ApplicationSettings Load()
    {
        LoadWarning = null;

        if (!File.Exists(FilePath))
            return CreateDefaults();

        try
        {
            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<ApplicationSettings>(json, JsonOptions)
                ?? throw new JsonException("The settings file is empty.");

            Normalize(settings);
            return settings;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            LoadWarning = $"The settings file could not be read, so default settings are being used.{Environment.NewLine}{Environment.NewLine}"
                + $"{FilePath}{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            return CreateDefaults();
        }
    }

    public void Save(ApplicationSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath) ?? ".");

        // Write to a temporary file first so a failed write never leaves a half-written settings file.
        var tempPath = FilePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tempPath, FilePath, overwrite: true);
    }

    private static SystemConfiguration CreateDefaultSystem(int number) => new() { SystemName = $"PC {number:00}" };

    // Repairs a hand-edited file: exactly five systems, no null values, no duplicate or unknown days.
    private static void Normalize(ApplicationSettings settings)
    {
        settings.AutomationDays = (settings.AutomationDays ?? [])
            .Where(day => Enum.IsDefined(day))
            .Distinct()
            .ToList();

        var systems = settings.Systems ?? [];
        if (systems.Count > ApplicationSettings.MaxSystems)
            systems.RemoveRange(ApplicationSettings.MaxSystems, systems.Count - ApplicationSettings.MaxSystems);

        while (systems.Count < ApplicationSettings.MaxSystems)
            systems.Add(CreateDefaultSystem(systems.Count + 1));

        for (var i = 0; i < systems.Count; i++)
        {
            var system = systems[i] ?? CreateDefaultSystem(i + 1);
            system.SystemName ??= string.Empty;
            system.ClassTeacherName ??= string.Empty;
            system.MainAlpPersonName ??= string.Empty;
            system.IntroVideoPath ??= string.Empty;
            system.PrayerVideoPath ??= string.Empty;
            systems[i] = system;
        }

        settings.Systems = systems;
        settings.ZoomExecutablePath ??= string.Empty;
    }
}
