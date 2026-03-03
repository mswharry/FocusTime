using FocusTime.Core.Helpers;
using System.Timers;

namespace FocusTime.Core.Services;

/// <summary>
/// Tracks the foreground application on Windows
/// </summary>
public class ForegroundAppTracker
{
    private System.Timers.Timer? _pollTimer;
    private string _lastProcessName = string.Empty;

    public event EventHandler<string>? ForegroundAppChanged;
    public string CurrentProcessName => _lastProcessName;

    /// <summary>
    /// Start tracking foreground app (polls every second)
    /// </summary>
    public void Start()
    {
        _pollTimer = new System.Timers.Timer(1000);
        _pollTimer.Elapsed += OnPollTick;
        _pollTimer.Start();
    }

    /// <summary>
    /// Stop tracking
    /// </summary>
    public void Stop()
    {
        if (_pollTimer != null)
        {
            _pollTimer.Stop();
            _pollTimer.Dispose();
            _pollTimer = null;
        }
    }

    private void OnPollTick(object? sender, ElapsedEventArgs e)
    {
        string processName = Win32Native.GetForegroundProcessName();
        
        if (!string.IsNullOrEmpty(processName) && processName != _lastProcessName)
        {
            _lastProcessName = processName;
            ForegroundAppChanged?.Invoke(this, processName);
        }
    }
}
