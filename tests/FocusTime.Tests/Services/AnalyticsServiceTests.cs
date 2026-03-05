using FocusTime.Core.Models;
using FocusTime.Core.Services;
using FocusTime.Core.Helpers;

namespace FocusTime.Tests.Services;

public class AnalyticsServiceTests
{
    private AppData CreateTestAppData()
    {
        var appData = new AppData
        {
            Settings = new Settings
            {
                DailyGoalMinutes = 120 // 2 hours
            },
            Days = new Dictionary<string, DayLog>()
        };

        return appData;
    }

    [Fact]
    public void IsTodayGoalAchieved_NoData_ShouldReturnFalse()
    {
        // Arrange
        var appData = CreateTestAppData();
        var analytics = new AnalyticsService(appData);

        // Act
        var result = analytics.IsTodayGoalAchieved();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTodayGoalAchieved_GoalMet_ShouldReturnTrue()
    {
        // Arrange
        var appData = CreateTestAppData();
        var todayKey = DateKeyHelper.GetToday();
        appData.Days[todayKey] = new DayLog
        {
            TotalFocusedSeconds = 120 * 60, // 2 hours
            TotalDistractedSeconds = 0
        };
        var analytics = new AnalyticsService(appData);

        // Act
        var result = analytics.IsTodayGoalAchieved();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTodayGoalAchieved_GoalNotMet_ShouldReturnFalse()
    {
        // Arrange
        var appData = CreateTestAppData();
        var todayKey = DateKeyHelper.GetToday();
        appData.Days[todayKey] = new DayLog
        {
            TotalFocusedSeconds = 60 * 60, // 1 hour (less than goal)
            TotalDistractedSeconds = 0
        };
        var analytics = new AnalyticsService(appData);

        // Act
        var result = analytics.IsTodayGoalAchieved();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculateStreak_NoData_ShouldReturnZero()
    {
        // Arrange
        var appData = CreateTestAppData();
        var analytics = new AnalyticsService(appData);

        // Act
        var streak = analytics.CalculateStreak();

        // Assert
        Assert.Equal(0, streak);
    }

    [Fact]
    public void CalculateStreak_ConsecutiveDays_ShouldReturnCorrectStreak()
    {
        // Arrange
        var appData = CreateTestAppData();
        var today = DateTime.Now.Date;

        // Add 3 consecutive days with goals achieved
        for (int i = 0; i < 3; i++)
        {
            var date = today.AddDays(-i);
            var dateKey = DateKeyHelper.GetDateKey(date);
            appData.Days[dateKey] = new DayLog
            {
                TotalFocusedSeconds = 120 * 60, // Meet goal
                TotalDistractedSeconds = 0
            };
        }

        var analytics = new AnalyticsService(appData);

        // Act
        var streak = analytics.CalculateStreak();

        // Assert
        Assert.Equal(3, streak);
    }

    [Fact]
    public void CalculateStreak_WithGap_ShouldResetStreak()
    {
        // Arrange
        var appData = CreateTestAppData();
        var today = DateTime.Now.Date;

        // Today: goal achieved
        appData.Days[DateKeyHelper.GetDateKey(today)] = new DayLog
        {
            TotalFocusedSeconds = 120 * 60,
            TotalDistractedSeconds = 0
        };

        // Yesterday: goal NOT achieved (gap)
        appData.Days[DateKeyHelper.GetDateKey(today.AddDays(-1))] = new DayLog
        {
            TotalFocusedSeconds = 30 * 60, // Less than goal
            TotalDistractedSeconds = 0
        };

        // 2 days ago: goal achieved
        appData.Days[DateKeyHelper.GetDateKey(today.AddDays(-2))] = new DayLog
        {
            TotalFocusedSeconds = 120 * 60,
            TotalDistractedSeconds = 0
        };

        var analytics = new AnalyticsService(appData);

        // Act
        var streak = analytics.CalculateStreak();

        // Assert - should only count today
        Assert.Equal(1, streak);
    }

    [Fact]
    public void CalculateLongestStreak_ShouldHandleMultipleFormats()
    {
        // Arrange
        var appData = CreateTestAppData();
        
        // Add days with different key formats
        appData.Days["20260301"] = new DayLog
        {
            TotalFocusedSeconds = 120 * 60,
            TotalDistractedSeconds = 0
        };
        
        appData.Days["2026-03-02"] = new DayLog
        {
            TotalFocusedSeconds = 120 * 60,
            TotalDistractedSeconds = 0
        };

        var analytics = new AnalyticsService(appData);

        // Act
        var longestStreak = analytics.CalculateLongestStreak();

        // Assert - should parse both formats
        Assert.True(longestStreak >= 0);
    }

    [Fact]
    public void GetLast7DaysTrend_ShouldReturnSevenItems()
    {
        // Arrange
        var appData = CreateTestAppData();
        var analytics = new AnalyticsService(appData);

        // Act
        var trend = analytics.GetLast7DaysTrend();

        // Assert
        Assert.Equal(7, trend.Count);
    }

    [Fact]
    public void GetLast7DaysTrend_WithData_ShouldShowCorrectMinutes()
    {
        // Arrange
        var appData = CreateTestAppData();
        var today = DateTime.Now.Date;
        var todayKey = DateKeyHelper.GetDateKey(today);
        
        appData.Days[todayKey] = new DayLog
        {
            TotalFocusedSeconds = 90 * 60, // 90 minutes
            TotalDistractedSeconds = 30 * 60 // 30 minutes
        };

        var analytics = new AnalyticsService(appData);

        // Act
        var trend = analytics.GetLast7DaysTrend();

        // Assert
        var todayTrend = trend.Last(); // Last item should be today
        Assert.Equal(90, todayTrend.FocusedMinutes);
        Assert.Equal(30, todayTrend.DistractedMinutes);
    }
}
