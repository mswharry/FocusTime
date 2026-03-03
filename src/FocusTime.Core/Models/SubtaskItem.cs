namespace FocusTime.Core.Models;

/// <summary>
/// Subtask within a TaskItem
/// </summary>
public class SubtaskItem
{
    public string SubtaskId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public bool Done { get; set; }
}
