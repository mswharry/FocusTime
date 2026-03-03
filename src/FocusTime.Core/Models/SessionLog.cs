namespace FocusTime.Core.Models;

/// <summary>
/// Log of a completed or active session
/// </summary>
public class SessionLog
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTimeLocal { get; set; }
    public DateTime? EndTimeLocal { get; set; }
    public int PlannedTotalMinutes { get; set; }
    public List<SegmentLog> Segments { get; set; } = new List<SegmentLog>();
    public List<TaskItem> TasksSnapshot { get; set; } = new List<TaskItem>();
    public string? ActiveTaskTitleAtStart { get; set; }
    public Dictionary<string, int> DomainSeconds { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> AppSeconds { get; set; } = new Dictionary<string, int>();
    public int FocusedSeconds { get; set; }
    public int DistractedSeconds { get; set; }
}
