namespace FocusTime.Core.Models;

/// <summary>
/// Application settings
/// </summary>
public class Settings
{
    private int _dailyGoalMinutes = 120;
    private int _domainTimeoutMinutes = 10;
    private int _blockedAllowedSecondsInWork = 90;
    private int _distractedRemindMinutes = 5;

    public int DailyGoalMinutes
    {
        get => _dailyGoalMinutes;
        set => _dailyGoalMinutes = Clamp(value, 10, 480);
    }

    public List<string> DomainBlocklist { get; set; } = new List<string>
    {
        "facebook.com",
        "youtube.com",
        "tiktok.com",
        "instagram.com",
        "reddit.com",
        "twitter.com",
        "x.com"
    };

    /// <summary>
    /// Domains considered work-related (focused time, not distracted)
    /// </summary>
    public List<string> DomainAllowlist { get; set; } = new List<string>
    {
        "github.com",
        "stackoverflow.com",
        "docs.microsoft.com",
        "developer.mozilla.org",
        "learn.microsoft.com",
        "google.com/search", // Google search results
        "bing.com/search"
    };

    public int DomainTimeoutMinutes
    {
        get => _domainTimeoutMinutes;
        set => _domainTimeoutMinutes = Clamp(value, 5, 120);
    }

    public int BlockedAllowedSecondsInWork
    {
        get => _blockedAllowedSecondsInWork;
        set => _blockedAllowedSecondsInWork = Clamp(value, 0, 300);
    }

    public int DistractedRemindMinutes
    {
        get => _distractedRemindMinutes;
        set => _distractedRemindMinutes = Clamp(value, 1, 30);
    }

    public List<string> AppAllowlistProcessNames { get; set; } = new List<string>
    {
        "Code.exe",
        "devenv.exe",
        "vmware.exe",
        "vmware-ui.exe",
        "WINWORD.EXE",
        "EXCEL.EXE",
        "POWERPNT.EXE",
        "notepad.exe",
        "notepad++.exe",
        "obsidian.exe",
        "rider64.exe",
        "pycharm64.exe"
    };

    /// <summary>
    /// Neutral apps - not counted as focused or distracted (system utilities)
    /// </summary>
    public List<string> AppNeutralProcessNames { get; set; } = new List<string>
    {
        "explorer.exe",
        "Calculator.exe",
        "Taskmgr.exe",
        "cmd.exe",
        "powershell.exe",
        "ApplicationFrameHost.exe" // UWP app host
    };

    public bool CountBreakBrowsingAsDistracted { get; set; } = false;

    /// <summary>
    /// Persisted domain timeouts: key = domain, value = timeout expiry (UTC)
    /// </summary>
    public Dictionary<string, DateTime> DomainTimeouts { get; set; } = new Dictionary<string, DateTime>();

    /// <summary>
    /// Validate all settings and clamp to valid ranges
    /// </summary>
    public void Validate()
    {
        DailyGoalMinutes = Clamp(_dailyGoalMinutes, 10, 480);
        DomainTimeoutMinutes = Clamp(_domainTimeoutMinutes, 5, 120);
        BlockedAllowedSecondsInWork = Clamp(_blockedAllowedSecondsInWork, 0, 300);
        DistractedRemindMinutes = Clamp(_distractedRemindMinutes, 1, 30);

        // Remove empty process names
        AppAllowlistProcessNames = AppAllowlistProcessNames
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        // Clean up expired timeouts
        if (DomainTimeouts != null)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = DomainTimeouts.Where(kvp => kvp.Value <= now).Select(kvp => kvp.Key).ToList();
            foreach (var key in expiredKeys)
            {
                DomainTimeouts.Remove(key);
            }
        }
        else
        {
            DomainTimeouts = new Dictionary<string, DateTime>();
        }
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
