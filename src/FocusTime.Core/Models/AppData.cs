namespace FocusTime.Core.Models;

/// <summary>
/// Root data structure for persistence
/// </summary>
public class AppData
{
    public int SchemaVersion { get; set; } = 1;
    public Settings Settings { get; set; } = new Settings();
    public Dictionary<string, DayLog> Days { get; set; } = new Dictionary<string, DayLog>();
}
