using Xunit;
using FocusTime.Core.Services;

namespace FocusTime.Tests.Services;

public class ScheduleBuilderTests
{
    [Theory]
    [InlineData(25, 5)] // 25 min: 10+2 cycles → W-B-W-B-W (5 segments)
    [InlineData(50, 4)] // 50 min: 20+5 cycles → W-B-W-B (4 segments)  
    [InlineData(90, 6)] // 90 min: 25+5 cycles → W-B-W-B-W-B (6 segments)
    public void Build_ShouldCreateCorrectNumberOfSegments(int totalMinutes, int expectedSegments)
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(totalMinutes);

        // Assert
        Assert.Equal(expectedSegments, segments.Count);
    }

    [Fact]
    public void Build_ShouldStartWithWorkSegment()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(25);

        // Assert
        Assert.Equal("Work", segments[0].Type);
    }

    [Fact]
    public void Build_ShouldAlternateWorkAndBreak()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(50);

        // Assert
        for (int i = 0; i < segments.Count; i++)
        {
            if (i % 2 == 0)
                Assert.Equal("Work", segments[i].Type);
            else
                Assert.Equal("Break", segments[i].Type);
        }
    }

    [Fact]
    public void Build_StandardPomodoro_WorkSegmentsShouldBe25Minutes()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(90); // Uses 25+5 profile

        // Assert
        var workSegments = segments.Where(s => s.Type == "Work");
        foreach (var segment in workSegments)
        {
            Assert.Equal(25, segment.Minutes);
        }
    }

    [Fact]
    public void Build_StandardPomodoro_BreakSegmentsShouldBe5Minutes()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(90); // Uses 25+5 profile

        // Assert
        var breakSegments = segments.Where(s => s.Type == "Break");
        foreach (var segment in breakSegments)
        {
            Assert.Equal(5, segment.Minutes);
        }
    }

    [Fact]
    public void Build_VeryShortSession_ShouldReturnSingleWorkSegment()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(10); // ≤ 15 → pure work

        // Assert
        Assert.Single(segments);
        Assert.Equal("Work", segments[0].Type);
        Assert.Equal(10, segments[0].Minutes);
    }

    [Fact]
    public void Build_ShortSession_ShouldUse10MinWorkCycles()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(25); // ≤ 30 → 10+2 profile

        // Assert
        var workSegments = segments.Where(s => s.Type == "Work" && s.Minutes == 10).ToList();
        Assert.Equal(2, workSegments.Count); // Two full 10-min work segments
    }

    [Fact]
    public void Build_MediumSession_ShouldUse20MinWorkCycles()
    {
        // Arrange
        var builder = new ScheduleBuilder();

        // Act
        var segments = builder.Build(50); // ≤ 60 → 20+5 profile

        // Assert
        var workSegments = segments.Where(s => s.Type == "Work");
        foreach (var segment in workSegments)
        {
            Assert.Equal(20, segment.Minutes);
        }
    }
}

