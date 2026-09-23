namespace SanStudioZoomAutomation.Configuration;

public sealed class SystemConfiguration
{
    public string SystemName { get; set; } = string.Empty;
    public string ClassTeacherName { get; set; } = string.Empty;
    public string MainAlpPersonName { get; set; } = string.Empty;
    public string IntroVideoPath { get; set; } = string.Empty;
    public string PrayerVideoPath { get; set; } = string.Empty;
    public TimeSpan IntroVideoDuration { get; set; }
    public TimeSpan PrayerVideoDuration { get; set; }
    public bool Enabled { get; set; } = true;
}
