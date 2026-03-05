using FocusTime.Core.Services;

namespace FocusTime.Tests.Services;

public class TimerEngineTests
{
    [Fact]
    public void Start_ShouldInitializeTimer()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(25); // 25 min Pomodoro

        // Act
        engine.Start(segments);

        // Assert
        Assert.True(engine.IsRunning);
        Assert.False(engine.IsPaused);
        Assert.Equal("Work", engine.CurrentPhase);
        Assert.True(engine.RemainingSeconds > 0);
    }

    [Fact]
    public void Pause_ShouldStopTimer()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(25);
        engine.Start(segments);

        // Act
        engine.Pause();

        // Assert
        Assert.False(engine.IsRunning);
        Assert.True(engine.IsPaused);
    }

    [Fact]
    public void Resume_ShouldRestartTimer()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(25);
        engine.Start(segments);
        engine.Pause();

        // Act
        engine.Resume();

        // Assert
        Assert.True(engine.IsRunning);
        Assert.False(engine.IsPaused);
    }

    [Fact]
    public void Stop_ShouldResetTimer()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(25);
        engine.Start(segments);

        // Act
        engine.Stop();

        // Assert
        Assert.False(engine.IsRunning);
        Assert.False(engine.IsPaused);
        Assert.Equal(0, engine.RemainingSeconds);
    }

    [Fact]
    public void PhaseChanged_ShouldFireEvent()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(1); // Quick 1 min session
        bool eventFired = false;
        string? newPhase = null;

        engine.PhaseChanged += (sender, e) =>
        {
            eventFired = true;
            newPhase = e.NewPhase;
        };

        // Act
        engine.Start(segments);

        // Assert
        Assert.True(eventFired);
        Assert.Equal("Work", newPhase);
    }

    [Fact]
    public void RemainingSeconds_ShouldBeAccurate()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(1); // 1 minute
        engine.Start(segments);

        int initialRemaining = engine.RemainingSeconds;

        // Act
        System.Threading.Thread.Sleep(1500); // Wait 1.5 seconds

        // Assert
        // Should be approximately 1-2 seconds less (timer precision)
        int currentRemaining = engine.RemainingSeconds;
        Assert.InRange(currentRemaining, initialRemaining - 3, initialRemaining);
    }

    [Fact]
    public void Dispose_ShouldCleanup()
    {
        // Arrange
        var engine = new TimerEngine();
        var scheduleBuilder = new ScheduleBuilder();
        var segments = scheduleBuilder.Build(25);
        engine.Start(segments);

        // Act
        engine.Dispose();

        // Assert
        Assert.False(engine.IsRunning);
    }
}
