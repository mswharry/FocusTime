using FocusTime.Core.Helpers;
using FocusTime.Core.Models;

namespace FocusTime.Core.Services;

/// <summary>
/// Analyzes daily goals, streaks, and trends
/// </summary>
public class AnalyticsService
{
    private readonly AppData _appData;

    public AnalyticsService(AppData appData)
    {
        _appData = appData;
    }

    /// <summary>
    /// Check if today's goal is achieved
    /// </summary>
    public bool IsTodayGoalAchieved()
    {
        var todayKey = DateKeyHelper.GetToday();
        if (!_appData.Days.ContainsKey(todayKey))
            return false;

        var focusedMinutes = _appData.Days[todayKey].TotalFocusedSeconds / 60;
        return focusedMinutes >= _appData.Settings.DailyGoalMinutes;
    }

    /// <summary>
    /// Calculate current streak (consecutive days achieving goal)
    /// </summary>
    public int CalculateStreak()
    {
        int streak = 0;
        var today = DateTime.Now.Date;

        for (int i = 0; i < 365; i++) // Check up to 1 year back
        {
            var checkDate = today.AddDays(-i);
            var dateKey = DateKeyHelper.GetDateKey(checkDate);

            if (!_appData.Days.ContainsKey(dateKey))
                break;

            var focusedMinutes = _appData.Days[dateKey].TotalFocusedSeconds / 60;
            if (focusedMinutes >= _appData.Settings.DailyGoalMinutes)
            {
                streak++;
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    /// <summary>
    /// Calculate longest streak ever achieved
    /// </summary>
    public int CalculateLongestStreak()
    {
        if (_appData.Days.Count == 0)
            return 0;

        int longestStreak = 0;
        int currentStreak = 0;

        // Get all dates sorted (handle multiple formats)
        var sortedDates = _appData.Days.Keys
            .Select(k =>
            {
                // Try primary format: yyyyMMdd
                if (DateTime.TryParseExact(k, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                    return (DateTime?)date;
                
                // Try alternate format: yyyy-MM-dd
                if (DateTime.TryParseExact(k, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date))
                    return (DateTime?)date;
                
                // Try general parse
                if (DateTime.TryParse(k, out date))
                    return (DateTime?)date;
                
                return null; // Invalid format, skip
            })
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .OrderBy(d => d)
            .ToList();

        DateTime? lastDate = null;

        foreach (var date in sortedDates)
        {
            var dateKey = DateKeyHelper.GetDateKey(date);
            
            // Handle case where key might not exist after format conversion
            if (!_appData.Days.ContainsKey(dateKey))
                continue;
                
            var focusedMinutes = _appData.Days[dateKey].TotalFocusedSeconds / 60;
            bool goalAchieved = focusedMinutes >= _appData.Settings.DailyGoalMinutes;

            if (goalAchieved)
            {
                // Check if consecutive day
                if (lastDate == null || (date - lastDate.Value).TotalDays == 1)
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    // Gap in streak, reset
                    currentStreak = 1;
                }
                lastDate = date;
            }
            else
            {
                // Goal not achieved, reset streak
                currentStreak = 0;
                lastDate = null;
            }
        }

        return longestStreak;
    }

    /// <summary>
    /// Get trend for last 7 days
    /// </summary>
    public List<DayTrend> GetLast7DaysTrend()
    {
        var trends = new List<DayTrend>();
        var today = DateTime.Now.Date;

        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dateKey = DateKeyHelper.GetDateKey(date);

            if (_appData.Days.ContainsKey(dateKey))
            {
                var day = _appData.Days[dateKey];
                trends.Add(new DayTrend
                {
                    Date = date,
                    FocusedMinutes = day.TotalFocusedSeconds / 60,
                    DistractedMinutes = day.TotalDistractedSeconds / 60,
                    GoalAchieved = (day.TotalFocusedSeconds / 60) >= _appData.Settings.DailyGoalMinutes
                });
            }
            else
            {
                trends.Add(new DayTrend
                {
                    Date = date,
                    FocusedMinutes = 0,
                    DistractedMinutes = 0,
                    GoalAchieved = false
                });
            }
        }

        return trends;
    }

    /// <summary>
    /// Get top distracting domains for a specific day
    /// </summary>
    public List<KeyValuePair<string, int>> GetTopDistractingDomains(string dateKey, int top = 5)
    {
        if (!_appData.Days.ContainsKey(dateKey))
            return new List<KeyValuePair<string, int>>();

        var domainTotals = new Dictionary<string, int>();
        foreach (var session in _appData.Days[dateKey].Sessions)
        {
            foreach (var kvp in session.DomainSeconds)
            {
                if (!domainTotals.ContainsKey(kvp.Key))
                    domainTotals[kvp.Key] = 0;
                domainTotals[kvp.Key] += kvp.Value;
            }
        }

        return domainTotals.OrderByDescending(x => x.Value).Take(top).ToList();
    }

    /// <summary>
    /// Get top distracting apps for a specific day
    /// </summary>
    public List<KeyValuePair<string, int>> GetTopDistractingApps(string dateKey, int top = 5)
    {
        if (!_appData.Days.ContainsKey(dateKey))
            return new List<KeyValuePair<string, int>>();

        var appTotals = new Dictionary<string, int>();
        foreach (var session in _appData.Days[dateKey].Sessions)
        {
            foreach (var kvp in session.AppSeconds)
            {
                if (!appTotals.ContainsKey(kvp.Key))
                    appTotals[kvp.Key] = 0;
                appTotals[kvp.Key] += kvp.Value;
            }
        }

        return appTotals.OrderByDescending(x => x.Value).Take(top).ToList();
    }
}

/// <summary>
/// Trend data for a single day
/// </summary>
public class DayTrend
{
    public DateTime Date { get; set; }
    public int FocusedMinutes { get; set; }
    public int DistractedMinutes { get; set; }
    public bool GoalAchieved { get; set; }
}
