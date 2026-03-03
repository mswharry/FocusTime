using FocusTime.Core.Models;
using FocusTime.Core.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FocusTime.App.ViewModels;

public class HistoryViewModel : INotifyPropertyChanged
{
    private readonly AppData _appData;
    private readonly AnalyticsService _analyticsService;

    public ObservableCollection<DayLogDisplay> DayLogs { get; } = new ObservableCollection<DayLogDisplay>();

    public HistoryViewModel(AppData appData)
    {
        _appData = appData;
        _analyticsService = new AnalyticsService(appData);
        LoadHistory();
    }

    private void LoadHistory()
    {
        var sortedDays = _appData.Days.OrderByDescending(x => x.Key).Take(30);
        
        foreach (var day in sortedDays)
        {
            var display = new DayLogDisplay
            {
                DateKey = day.Key,
                FocusedMinutes = day.Value.TotalFocusedSeconds / 60,
                DistractedMinutes = day.Value.TotalDistractedSeconds / 60,
                SessionCount = day.Value.Sessions.Count,
                GoalAchieved = (day.Value.TotalFocusedSeconds / 60) >= _appData.Settings.DailyGoalMinutes
            };

            // Top domains
            var topDomains = _analyticsService.GetTopDistractingDomains(day.Key, 3);
            display.TopDomains = string.Join(", ", topDomains.Select(x => x.Key));

            DayLogs.Add(display);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class DayLogDisplay
{
    public string DateKey { get; set; } = string.Empty;
    public int FocusedMinutes { get; set; }
    public int DistractedMinutes { get; set; }
    public int SessionCount { get; set; }
    public bool GoalAchieved { get; set; }
    public string TopDomains { get; set; } = string.Empty;
}
