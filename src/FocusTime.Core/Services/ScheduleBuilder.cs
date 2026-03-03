namespace FocusTime.Core.Services;

/// <summary>
/// Represents a work or break segment in a session schedule
/// </summary>
public class ScheduleSegment
{
    public string Type { get; set; } = string.Empty; // "Work" or "Break"
    public int Minutes { get; set; }
}

/// <summary>
/// Builds work/break schedule based on total session minutes
/// </summary>
public class ScheduleBuilder
{
    /// <summary>
    /// Generate schedule segments based on total minutes
    /// </summary>
    public List<ScheduleSegment> Build(int totalMinutes)
    {
        var segments = new List<ScheduleSegment>();

        // Select profile based on total time
        int work, breakTime;
        
        // Very short sessions (10-20 min): full work, no break or minimal break
        if (totalMinutes <= 15)
        {
            // Pure focus session - no breaks
            segments.Add(new ScheduleSegment { Type = "Work", Minutes = totalMinutes });
            return segments;
        }
        else if (totalMinutes <= 30)
        {
            // Short session: 10 min work, 2 min break cycles
            work = 10;
            breakTime = 2;
        }
        else if (totalMinutes <= 60)
        {
            // Standard short: 20 min work, 5 min break
            work = 20;
            breakTime = 5;
        }
        else if (totalMinutes <= 120)
        {
            // Standard medium: 25 min work, 5 min break (Pomodoro-style)
            work = 25;
            breakTime = 5;
        }
        else
        {
            // Long session: 50 min work, 10 min break
            work = 50;
            breakTime = 10;
        }

        int remaining = totalMinutes;

        // Build cycles
        while (remaining >= work + breakTime)
        {
            segments.Add(new ScheduleSegment { Type = "Work", Minutes = work });
            segments.Add(new ScheduleSegment { Type = "Break", Minutes = breakTime });
            remaining -= (work + breakTime);
        }

        // Handle remainder
        if (remaining > 0)
        {
            if (remaining <= work)
            {
                segments.Add(new ScheduleSegment { Type = "Work", Minutes = remaining });
            }
            else
            {
                segments.Add(new ScheduleSegment { Type = "Work", Minutes = work });
                segments.Add(new ScheduleSegment { Type = "Break", Minutes = remaining - work });
            }
        }

        return segments;
    }
}
