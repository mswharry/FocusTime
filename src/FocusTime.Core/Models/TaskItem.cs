namespace FocusTime.Core.Models;

/// <summary>
/// A task item for a session
/// </summary>
public class TaskItem
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public int? EstimateMinutes { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public int Priority { get; set; } = 2; // 1=High, 2=Medium, 3=Low
    public string Status { get; set; } = "Todo"; // Todo, Doing, Done, Partial, Blocked
    public List<SubtaskItem>? Subtasks { get; set; }
    public string? Note { get; set; }
}
