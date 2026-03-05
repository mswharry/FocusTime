using FocusTime.Core.Models;

namespace FocusTime.Tests.Models;

public class SettingsTests
{
    [Fact]
    public void Validate_DailyGoalMinutes_ShouldClampToRange()
    {
        // Arrange
        var settings = new Settings
        {
            DailyGoalMinutes = 5 // Below minimum (10)
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(10, settings.DailyGoalMinutes);
    }

    [Fact]
    public void Validate_DailyGoalMinutes_TooHigh_ShouldClampToMax()
    {
        // Arrange
        var settings = new Settings
        {
            DailyGoalMinutes = 500 // Above maximum (480)
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(480, settings.DailyGoalMinutes);
    }

    [Fact]
    public void Validate_DomainTimeoutMinutes_ShouldClampToRange()
    {
        // Arrange
        var settings = new Settings
        {
            DomainTimeoutMinutes = 2 // Below minimum (5)
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(5, settings.DomainTimeoutMinutes);
    }

    [Fact]
    public void Validate_BlockedAllowedSecondsInWork_ShouldClampToRange()
    {
        // Arrange
        var settings = new Settings
        {
            BlockedAllowedSecondsInWork = -10 // Below minimum (0)
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(0, settings.BlockedAllowedSecondsInWork);
    }

    [Fact]
    public void Validate_BlockedAllowedSecondsInWork_TooHigh_ShouldClampToMax()
    {
        // Arrange
        var settings = new Settings
        {
            BlockedAllowedSecondsInWork = 400 // Above maximum (300)
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(300, settings.BlockedAllowedSecondsInWork);
    }

    [Fact]
    public void Validate_DistractedRemindMinutes_ShouldClampToRange()
    {
        // Arrange
        var settings = new Settings
        {
            DistractedRemindMinutes = 0 // Below minimum (1)
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(1, settings.DistractedRemindMinutes);
    }

    [Fact]
    public void Validate_ShouldRemoveExpiredTimeouts()
    {
        // Arrange
        var settings = new Settings();
        settings.DomainTimeouts["expired.com"] = DateTime.UtcNow.AddMinutes(-10);
        settings.DomainTimeouts["active.com"] = DateTime.UtcNow.AddMinutes(10);

        // Act
        settings.Validate();

        // Assert
        Assert.False(settings.DomainTimeouts.ContainsKey("expired.com"));
        Assert.True(settings.DomainTimeouts.ContainsKey("active.com"));
    }

    [Fact]
    public void Validate_ValidSettings_ShouldNotChange()
    {
        // Arrange
        var settings = new Settings
        {
            DailyGoalMinutes = 120,
            DomainTimeoutMinutes = 10,
            BlockedAllowedSecondsInWork = 30,
            DistractedRemindMinutes = 5
        };

        // Act
        settings.Validate();

        // Assert
        Assert.Equal(120, settings.DailyGoalMinutes);
        Assert.Equal(10, settings.DomainTimeoutMinutes);
        Assert.Equal(30, settings.BlockedAllowedSecondsInWork);
        Assert.Equal(5, settings.DistractedRemindMinutes);
    }
}
