using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FocusTime.Core.Services;

namespace FocusTime.App.ViewModels;

/// <summary>
/// ViewModel for session timer management
/// </summary>
public class SessionViewModel : INotifyPropertyChanged
{
    private readonly TimerEngine _timerEngine;
    private readonly ScheduleBuilder _scheduleBuilder;
    
    private string _timerDisplay = "00:00";
    private string _phaseDisplay = "Ready";
    private bool _isSessionActive = false;
    private int _selectedPresetIndex = 1; // Default: 90 min
    private List<ScheduleSegment> _currentSegments = new List<ScheduleSegment>();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<PhaseChangedEventArgs>? PhaseChanged;
    public event EventHandler? SessionCompleted;
    public event EventHandler? Tick;

    public SessionViewModel(TimerEngine timerEngine, ScheduleBuilder scheduleBuilder)
    {
        _timerEngine = timerEngine;
        _scheduleBuilder = scheduleBuilder;

        // Wire up timer events
        _timerEngine.Tick += OnTimerTick;
        _timerEngine.PhaseChanged += OnPhaseChanged;
        _timerEngine.SessionCompleted += OnSessionCompleted;

        // Initialize presets
        SessionPresets = new ObservableCollection<string>
        {
            "10 min", "15 min", "20 min", "30 min", "45 min", "60 min", "90 min", "120 min", 
            "150 min", "180 min", "210 min", "240 min"
        };

        // Commands
        StartCommand = new RelayCommand(StartSession, () => !_isSessionActive);
        PauseCommand = new RelayCommand(PauseSession, () => _isSessionActive && !_timerEngine.IsPaused);
        ResumeCommand = new RelayCommand(ResumeSession, () => _isSessionActive && _timerEngine.IsPaused);
        ResetCommand = new RelayCommand(ResetSession, () => _isSessionActive);
        SkipBreakCommand = new RelayCommand(SkipBreak, () => _isSessionActive && _phaseDisplay == "BREAK");
    }

    public ObservableCollection<string> SessionPresets { get; }

    public string TimerDisplay
    {
        get => _timerDisplay;
        set => SetProperty(ref _timerDisplay, value);
    }

    public string PhaseDisplay
    {
        get => _phaseDisplay;
        set => SetProperty(ref _phaseDisplay, value);
    }

    public bool IsSessionActive
    {
        get => _isSessionActive;
        set => SetProperty(ref _isSessionActive, value);
    }

    public int SelectedPresetIndex
    {
        get => _selectedPresetIndex;
        set => SetProperty(ref _selectedPresetIndex, value);
    }

    public TimerEngine TimerEngine => _timerEngine;
    public List<ScheduleSegment> CurrentSegments => _currentSegments;

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SkipBreakCommand { get; }

    private void StartSession()
    {
        int selectedMinutes = GetSelectedMinutes();
        if (selectedMinutes <= 0) return;

        _currentSegments = _scheduleBuilder.Build(selectedMinutes);
        _timerEngine.Start(_currentSegments);
        IsSessionActive = true;

        UpdateTimerDisplay();
        RaiseCommandsCanExecuteChanged();
    }

    private void PauseSession()
    {
        _timerEngine.Pause();
        RaiseCommandsCanExecuteChanged();
    }

    private void ResumeSession()
    {
        _timerEngine.Resume();
        RaiseCommandsCanExecuteChanged();
    }

    private void ResetSession()
    {
        _timerEngine.Stop();
        IsSessionActive = false;
        TimerDisplay = "00:00";
        PhaseDisplay = "Ready";
        _currentSegments.Clear();
        RaiseCommandsCanExecuteChanged();
    }

    private void SkipBreak()
    {
        _timerEngine.SkipBreak();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateTimerDisplay();
        Tick?.Invoke(this, EventArgs.Empty);
    }

    private void OnPhaseChanged(object? sender, PhaseChangedEventArgs e)
    {
        PhaseDisplay = e.NewPhase.ToUpper();
        PhaseChanged?.Invoke(this, e);
    }

    private void OnSessionCompleted(object? sender, EventArgs e)
    {
        IsSessionActive = false;
        PhaseDisplay = "COMPLETED";
        RaiseCommandsCanExecuteChanged();
        SessionCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTimerDisplay()
    {
        int remaining = _timerEngine.RemainingSeconds;
        int minutes = remaining / 60;
        int seconds = remaining % 60;
        TimerDisplay = $"{minutes:D2}:{seconds:D2}";
    }

    private int GetSelectedMinutes()
    {
        var presetText = SessionPresets[SelectedPresetIndex];
        var minutesStr = presetText.Replace(" min", "");
        return int.TryParse(minutesStr, out int minutes) ? minutes : 0;
    }

    public void RestoreState(List<ScheduleSegment> segments, int currentSegmentIndex, int remainingSeconds)
    {
        _currentSegments = segments;
        _timerEngine.Start(segments);
        // Note: TimerEngine would need methods to restore internal state
        IsSessionActive = true;
        UpdateTimerDisplay();
    }

    private void RaiseCommandsCanExecuteChanged()
    {
        (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PauseCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResumeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SkipBreakCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
