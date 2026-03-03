using System.Timers;

namespace FocusTime.Core.Services;

/// <summary>
/// Timer engine with phase management and events
/// </summary>
public class TimerEngine
{
    private System.Timers.Timer? _timer;
    private List<ScheduleSegment> _segments = new List<ScheduleSegment>();
    private int _currentSegmentIndex = 0;
    private int _remainingSeconds = 0;
    private bool _isPaused = false;

    public event EventHandler<PhaseChangedEventArgs>? PhaseChanged;
    public event EventHandler? SegmentCompleted;
    public event EventHandler? SessionCompleted;
    public event EventHandler? Tick;

    public bool IsRunning => _timer?.Enabled ?? false;
    public bool IsPaused => _isPaused;
    public string CurrentPhase => _currentSegmentIndex < _segments.Count ? _segments[_currentSegmentIndex].Type : "";
    public int RemainingSeconds => _remainingSeconds;
    public int CurrentSegmentIndex => _currentSegmentIndex;
    public int TotalSegments => _segments.Count;

    /// <summary>
    /// Start a new session with the given schedule
    /// </summary>
    public void Start(List<ScheduleSegment> segments)
    {
        Stop();
        _segments = segments;
        _currentSegmentIndex = 0;
        _isPaused = false;

        if (_segments.Count > 0)
        {
            _remainingSeconds = _segments[0].Minutes * 60;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerTick;
            _timer.Start();

            PhaseChanged?.Invoke(this, new PhaseChangedEventArgs(_segments[0].Type));
        }
    }

    /// <summary>
    /// Pause the timer
    /// </summary>
    public void Pause()
    {
        if (_timer != null && !_isPaused)
        {
            _timer.Stop();
            _isPaused = true;
        }
    }

    /// <summary>
    /// Resume the timer
    /// </summary>
    public void Resume()
    {
        if (_timer != null && _isPaused)
        {
            _timer.Start();
            _isPaused = false;
        }
    }

    /// <summary>
    /// Stop the timer completely
    /// </summary>
    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
        _isPaused = false;
        _segments.Clear();
        _currentSegmentIndex = 0;
        _remainingSeconds = 0;
    }

    /// <summary>
    /// Skip the current break segment (only works during Break phase)
    /// </summary>
    public void SkipBreak()
    {
        if (CurrentPhase == "Break")
        {
            _remainingSeconds = 0; // Force advance to next segment
        }
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        _remainingSeconds--;
        Tick?.Invoke(this, EventArgs.Empty);

        if (_remainingSeconds <= 0)
        {
            SegmentCompleted?.Invoke(this, EventArgs.Empty);
            _currentSegmentIndex++;

            if (_currentSegmentIndex >= _segments.Count)
            {
                // Session completed
                Stop();
                SessionCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Move to next segment
                _remainingSeconds = _segments[_currentSegmentIndex].Minutes * 60;
                PhaseChanged?.Invoke(this, new PhaseChangedEventArgs(_segments[_currentSegmentIndex].Type));
            }
        }
    }
}

/// <summary>
/// Event args for phase changes
/// </summary>
public class PhaseChangedEventArgs : EventArgs
{
    public string NewPhase { get; }

    public PhaseChangedEventArgs(string newPhase)
    {
        NewPhase = newPhase;
    }
}
