namespace FocusTime.Core.Services;

/// <summary>
/// Manages distraction prompt timing and grace windows
/// </summary>
public class DistractionPromptService
{
    private DateTime? _lastPromptTime;
    private Dictionary<string, DateTime> _graceWindows = new Dictionary<string, DateTime>();
    private const int MinPromptIntervalSeconds = 30; // Prevent spam

    /// <summary>
    /// Check if should show prompt for an app/domain
    /// </summary>
    public bool ShouldShowPrompt(string source)
    {
        // Don't prompt if recently prompted
        if (_lastPromptTime!= null && (DateTime.Now - _lastPromptTime.Value).TotalSeconds < MinPromptIntervalSeconds)
            return false;

        // Don't prompt if in grace window
        if (IsInGraceWindow(source))
            return false;

        return true;
    }

    /// <summary>
    /// Mark that prompt was shown
    /// </summary>
    public void MarkPromptShown(string source)
    {
        _lastPromptTime = DateTime.Now;
    }

    /// <summary>
    /// Set grace window for an app/domain
    /// </summary>
    public void SetGraceWindow(string source, int minutes)
    {
        _graceWindows[source] = DateTime.Now.AddMinutes(minutes);
    }

    /// <summary>
    /// Check if source is in grace window
    /// </summary>
    public bool IsInGraceWindow(string source)
    {
        if (_graceWindows.ContainsKey(source))
        {
            if (DateTime.Now < _graceWindows[source])
                return true;
            else
                _graceWindows.Remove(source); // Expired
        }
        return false;
    }

    /// <summary>
    /// Clear all grace windows (e.g., on new work segment)
    /// </summary>
    public void ClearGraceWindows()
    {
        _graceWindows.Clear();
        _lastPromptTime = null;
    }
}

/// <summary>
/// User response to distraction prompt
/// </summary>
public class DistractionPromptResult
{
    public DistractionPurpose Purpose { get; set; }
    public int DurationMinutes { get; set; }
    public bool WasDismissed { get; set; }
}

public enum DistractionPurpose
{
    Work,      // Liên quan công việc
    Needed,    // Việc cần thiết
    Break,     // Nghỉ giải lao
    Wandering  // Lạc
}
