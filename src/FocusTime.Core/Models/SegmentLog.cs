namespace FocusTime.Core.Models;

/// <summary>
/// Log of a Work or Break segment within a session
/// </summary>
public class SegmentLog
{
    public string Type { get; set; } = string.Empty; // "Work" or "Break"
    public int PlannedSeconds { get; set; }
    public int ActualSeconds { get; set; }
    public int BreakBankAppliedSeconds { get; set; } // For break segments
}
