using System.Timers;
using System.Diagnostics;

namespace FocusTime.Core.Services;

/// <summary>
/// Timer engine with phase management and events
/// </summary>
public class TimerEngine : IDisposable
{
    private System.Timers.Timer? _timer;
    private List<ScheduleSegment> _segments = new List<ScheduleSegment>();
    private int _currentSegmentIndex = 0;
    private int _remainingSeconds = 0;
    private DateTime _segmentEndTime;
    private bool _isPaused = false;
    private bool _disposed = false;

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
            _segmentEndTime = DateTime.Now.AddSeconds(_remainingSeconds);
            
            _timer = new System.Timers.Timer(500); // Use 500ms interval for more responsive UI
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
            // Save remaining seconds at pause time
            _remainingSeconds = Math.Max(0, (int)(_segmentEndTime - DateTime.Now).TotalSeconds);
        }
    }

    /// <summary>
    /// Resume the timer
    /// </summary>
    public void Resume()
    {
        if (_timer != null && _isPaused)
        {
            // Recalculate end time based on remaining seconds
            _segmentEndTime = DateTime.Now.AddSeconds(_remainingSeconds);
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

    /// <summary>
    /// Adjust the current segment's remaining time (e.g., for break bank penalty)
    /// </summary>
    /// <param name="newSeconds">New remaining seconds for current segment</param>
    public void AdjustCurrentSegmentTime(int newSeconds)
    {
        if (_timer == null || _isPaused)
            return;

        if (newSeconds < 0)
            newSeconds = 0;

        _remainingSeconds = newSeconds;
        _segmentEndTime = DateTime.Now.AddSeconds(_remainingSeconds);
    }

    /// <summary>
    /// Set exact remaining time for current segment (mainly for session recovery)
    /// </summary>
    /// <param name="seconds">Exact remaining seconds</param>
    public void SetRemainingTime(int seconds)
    {
        if (_timer == null)
            return;

        _remainingSeconds = Math.Max(0, seconds);
        _segmentEndTime = DateTime.Now.AddSeconds(_remainingSeconds);
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        // Calculate remaining seconds based on actual elapsed time
        _remainingSeconds = Math.Max(0, (int)(_segmentEndTime - DateTime.Now).TotalSeconds);
        
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
                _segmentEndTime = DateTime.Now.AddSeconds(_remainingSeconds);
                PhaseChanged?.Invoke(this, new PhaseChangedEventArgs(_segments[_currentSegmentIndex].Type));
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop();
        }

        _disposed = true;
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
