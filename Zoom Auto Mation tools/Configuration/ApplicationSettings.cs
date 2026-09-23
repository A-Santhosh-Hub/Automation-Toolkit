namespace SanStudioZoomAutomation.Configuration;

public sealed class ApplicationSettings
{
    public const int MinSystems = 1;
    public const int MaxSystems = 5;

    public const int MinPreparationMinutes = 1;
    public const int MaxPreparationMinutes = 120;
    public const int DefaultPreparationMinutes = 10;

    public static readonly DayOfWeek[] WeekOrder =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday,
    ];

    public List<DayOfWeek> AutomationDays { get; set; } =
        WeekOrder.Where(day => day != DayOfWeek.Sunday).ToList();

    public TimeSpan MeetingStartTime { get; set; } = new(4, 30, 0);
    public TimeSpan ExpectedEndTime { get; set; } = new(8, 30, 0);

    // How many minutes before the meeting start the automation session begins preparing.
    public int PreparationMinutes { get; set; } = DefaultPreparationMinutes;

    // Full path to Zoom.exe. Empty means the application finds Zoom itself, which is the normal case.
    public string ZoomExecutablePath { get; set; } = string.Empty;

    public int NumberOfSystems { get; set; } = MaxSystems;
    public List<SystemConfiguration> Systems { get; set; } = [];
}
