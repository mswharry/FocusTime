using FocusTime.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocusTime.App.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly Settings _settings;
    private static readonly List<string> DefaultDomains = new()
    {
        "facebook.com", "youtube.com", "tiktok.com", "instagram.com", "reddit.com"
    };
    private static readonly List<string> DefaultApps = new()
    {
        "Code.exe", "devenv.exe", "WINWORD.EXE", "EXCEL.EXE", 
        "POWERPNT.EXE", "notepad.exe", "obsidian.exe"
    };

    public SettingsViewModel(Settings settings)
    {
        _settings = settings;
        
        DailyGoalMinutes = _settings.DailyGoalMinutes;
        DomainTimeoutMinutes = _settings.DomainTimeoutMinutes;
        BlockedAllowedSeconds = _settings.BlockedAllowedSecondsInWork;
        DistractedRemindMinutes = _settings.DistractedRemindMinutes;
        
        DomainBlocklist = new ObservableCollection<string>(_settings.DomainBlocklist);
        AppAllowlist = new ObservableCollection<string>(_settings.AppAllowlistProcessNames);
    }

    private int _dailyGoalMinutes;
    public int DailyGoalMinutes
    {
        get => _dailyGoalMinutes;
        set => SetProperty(ref _dailyGoalMinutes, value);
    }

    private int _domainTimeoutMinutes;
    public int DomainTimeoutMinutes
    {
        get => _domainTimeoutMinutes;
        set => SetProperty(ref _domainTimeoutMinutes, value);
    }

    private int _blockedAllowedSeconds;
    public int BlockedAllowedSeconds
    {
        get => _blockedAllowedSeconds;
        set => SetProperty(ref _blockedAllowedSeconds, value);
    }

    private int _distractedRemindMinutes;
    public int DistractedRemindMinutes
    {
        get => _distractedRemindMinutes;
        set => SetProperty(ref _distractedRemindMinutes, value);
    }

    public ObservableCollection<string> DomainBlocklist { get; }
    public ObservableCollection<string> AppAllowlist { get; }

    private string? _selectedDomain;
    public string? SelectedDomain
    {
        get => _selectedDomain;
        set => SetProperty(ref _selectedDomain, value);
    }

    private string? _selectedApp;
    public string? SelectedApp
    {
        get => _selectedApp;
        set => SetProperty(ref _selectedApp, value);
    }

    public bool CanRemoveDomain(string domain)
    {
        return !DefaultDomains.Contains(domain);
    }

    public bool CanRemoveApp(string app)
    {
        return !DefaultApps.Contains(app);
    }

    public void AddDomain(string domain)
    {
        if (!string.IsNullOrWhiteSpace(domain) && !DomainBlocklist.Contains(domain))
        {
            DomainBlocklist.Add(domain);
        }
    }

    public void RemoveDomain(string domain)
    {
        if (CanRemoveDomain(domain))
        {
            DomainBlocklist.Remove(domain);
        }
    }

    public void AddApp(string app)
    {
        if (!string.IsNullOrWhiteSpace(app) && !AppAllowlist.Contains(app))
        {
            AppAllowlist.Add(app);
        }
    }

    public void RemoveApp(string app)
    {
        if (CanRemoveApp(app))
        {
            AppAllowlist.Remove(app);
        }
    }

    public void SaveToSettings()
    {
        _settings.DailyGoalMinutes = DailyGoalMinutes;
        _settings.DomainTimeoutMinutes = DomainTimeoutMinutes;
        _settings.BlockedAllowedSecondsInWork = BlockedAllowedSeconds;
        _settings.DistractedRemindMinutes = DistractedRemindMinutes;
        _settings.DomainBlocklist = new List<string>(DomainBlocklist);
        _settings.AppAllowlistProcessNames = new List<string>(AppAllowlist);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
