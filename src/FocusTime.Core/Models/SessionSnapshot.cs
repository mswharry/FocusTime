using FocusTime.Core.Services;

namespace FocusTime.Core.Models;

/// <summary>
/// Snapshot of a session for crash recovery
/// </summary>
public class SessionSnapshot
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime SnapshotTime { get; set; }
    public int PlannedTotalMinutes { get; set; }
    public int CurrentSegmentIndex { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public int RemainingSecondsInSegment { get; set; }
    public int FocusedSeconds { get; set; }
    public int DistractedSeconds { get; set; }
    public string? ActiveTaskTitle { get; set; }
    public List<TaskItem> TasksSnapshot { get; set; } = new List<TaskItem>();
    public Dictionary<string, int> DomainSeconds { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> AppSeconds { get; set; } = new Dictionary<string, int>();
    public bool InProgress { get; set; } = true;
    public List<ScheduleSegment> Segments { get; set; } = new List<ScheduleSegment>();
    public int ContinuousDistractedSeconds { get; set; }
    public int ContinuousFocusedSeconds { get; set; }
}
