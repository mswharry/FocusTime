using Xunit;
using FocusTime.Core.Models;
using FocusTime.Core.Services;

namespace FocusTime.Tests.Services;

public class DistractionPolicyEngineTests
{
    [Fact]
    public void IsDomainBlocked_NoTimeout_ReturnsFalse()
    {
        // Arrange
        var settings = new Settings();
        settings.DomainBlocklist.Add("facebook.com");
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.IsDomainBlocked("facebook.com", out var timeoutUntil);

        // Assert
        Assert.False(result);
        Assert.Null(timeoutUntil);
    }

    [Fact]
    public void IsDomainBlocked_WithValidTimeout_ReturnsTrue()
    {
        // Arrange
        var settings = new Settings();
        var futureTime = DateTime.UtcNow.AddMinutes(10);
        settings.DomainTimeouts["youtube.com"] = futureTime;
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.IsDomainBlocked("youtube.com", out var timeoutUntil);

        // Assert
        Assert.True(result);
        Assert.NotNull(timeoutUntil);
        Assert.Equal(futureTime, timeoutUntil.Value);
    }

    [Fact]
    public void IsDomainBlocked_WithExpiredTimeout_ReturnsFalseAndCleansUp()
    {
        // Arrange
        var settings = new Settings();
        settings.DomainTimeouts["twitter.com"] = DateTime.UtcNow.AddMinutes(-1);
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.IsDomainBlocked("twitter.com", out var timeoutUntil);

        // Assert
        Assert.False(result);
        Assert.Null(timeoutUntil);
        Assert.False(settings.DomainTimeouts.ContainsKey("twitter.com")); // Should be cleaned up
    }

    [Fact]
    public void TrackDomainUsage_NotInBlocklist_ReturnsFalse()
    {
        // Arrange
        var settings = new Settings();
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.TrackDomainUsage("google.com", isWorkPhase: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TrackDomainUsage_NotInWorkPhase_ReturnsFalse()
    {
        // Arrange
        var settings = new Settings();
        settings.DomainBlocklist.Add("reddit.com");
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.TrackDomainUsage("reddit.com", isWorkPhase: false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TrackDomainUsage_ExceedsThreshold_ReturnsTrueAndSetsTimeout()
    {
        // Arrange
        var settings = new Settings
        {
            BlockedAllowedSecondsInWork = 5,
            DomainTimeoutMinutes = 15
        };
        settings.DomainBlocklist.Add("facebook.com");
        var engine = new DistractionPolicyEngine(settings);

        // Act - track 6 seconds (exceeds threshold of 5)
        bool result = false;
        for (int i = 0; i < 6; i++)
        {
            result = engine.TrackDomainUsage("facebook.com", isWorkPhase: true);
        }

        // Assert
        Assert.True(result);
        Assert.True(settings.DomainTimeouts.ContainsKey("facebook.com"));
        Assert.True(engine.IsDomainBlocked("facebook.com", out _));
    }

    [Fact]
    public void TrackDomainUsage_AlreadyTimedOut_ReturnsTrue()
    {
        // Arrange
        var settings = new Settings();
        settings.DomainBlocklist.Add("youtube.com");
        settings.DomainTimeouts["youtube.com"] = DateTime.UtcNow.AddMinutes(10);
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.TrackDomainUsage("youtube.com", isWorkPhase: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ResetWorkSegment_ClearsTracking()
    {
        // Arrange
        var settings = new Settings
        {
            BlockedAllowedSecondsInWork = 10
        };
        settings.DomainBlocklist.Add("twitter.com");
        var engine = new DistractionPolicyEngine(settings);

        // Track 3 seconds
        for (int i = 0; i < 3; i++)
        {
            engine.TrackDomainUsage("twitter.com", isWorkPhase: true);
        }

        // Act
        engine.ResetWorkSegment();

        // Track 3 more seconds - should not trigger timeout (threshold is 10)
        bool triggered = false;
        for (int i = 0; i < 3; i++)
        {
            triggered = engine.TrackDomainUsage("twitter.com", isWorkPhase: true);
        }

        // Assert
        Assert.False(triggered); // Should not trigger because tracking was reset
    }

    [Fact]
    public void IsAppAllowed_AppInAllowlist_ReturnsTrue()
    {
        // Arrange
        var settings = new Settings();
        settings.AppAllowlistProcessNames.Add("vscode");
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.IsAppAllowed("vscode");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAppAllowed_AppNotInAllowlist_ReturnsFalse()
    {
        // Arrange
        var settings = new Settings();
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.IsAppAllowed("chrome");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ComputeBreakBank_NoDistractions_ReturnsFullBreak()
    {
        // Arrange
        var settings = new Settings();
        var engine = new DistractionPolicyEngine(settings);

        // Act
        var result = engine.ComputeBreakBank(plannedBreakMinutes: 10, distractedMinutesInPrevWork: 0);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void ComputeBreakBank_WithDistractions_ReturnsDiscountedBreak()
    {
        // Arrange
        var settings = new Settings();
        var engine = new DistractionPolicyEngine(settings);

        // Act - 10 min distracted / 2 = 5 min discount
        var result = engine.ComputeBreakBank(plannedBreakMinutes: 10, distractedMinutesInPrevWork: 10);

        // Assert
        Assert.Equal(5, result); // 10 - 5 = 5
    }

    [Fact]
    public void ComputeBreakBank_HighDistractions_ReturnsZero()
    {
        // Arrange
        var settings = new Settings();
        var engine = new DistractionPolicyEngine(settings);

        // Act - 30 min distracted / 2 = 15 min discount, exceeds 10 min planned
        var result = engine.ComputeBreakBank(plannedBreakMinutes: 10, distractedMinutesInPrevWork: 30);

        // Assert
        Assert.Equal(0, result); // Cannot go below 0
    }
}
