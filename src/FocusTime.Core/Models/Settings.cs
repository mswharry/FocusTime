namespace FocusTime.Core.Models;

/// <summary>
/// Application settings
/// </summary>
public class Settings
{
    public int DailyGoalMinutes { get; set; } = 120;
    public List<string> DomainBlocklist { get; set; } = new List<string>
    {
        "facebook.com",
        "youtube.com",
        "tiktok.com",
        "instagram.com",
        "reddit.com"
    };
    public int DomainTimeoutMinutes { get; set; } = 10;
    public int BlockedAllowedSecondsInWork { get; set; } = 90;
    public int DistractedRemindMinutes { get; set; } = 5;
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
        "obsidian.exe"
    };
    public bool CountBreakBrowsingAsDistracted { get; set; } = false;
}
