namespace FocusTime.Core.Models;

/// <summary>
/// Daily log with all sessions for a specific date
/// </summary>
public class DayLog
{
    public string DateKey { get; set; } = string.Empty; // "YYYY-MM-DD"
    public List<SessionLog> Sessions { get; set; } = new List<SessionLog>();
    public int TotalFocusedSeconds { get; set; }
    public int TotalDistractedSeconds { get; set; }
}
