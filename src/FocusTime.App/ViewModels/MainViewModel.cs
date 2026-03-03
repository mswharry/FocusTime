using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FocusTime.Core.Models;
using FocusTime.Core.Services;
using FocusTime.Core.Helpers;
using FocusTime.App.Models;

namespace FocusTime.App.ViewModels;

/// <summary>
/// Main view model for the main window
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly TimerEngine _timerEngine;
    private readonly ScheduleBuilder _scheduleBuilder;
    private readonly ForegroundAppTracker _foregroundTracker;
    private readonly DistractionPolicyEngine _policyEngine;
    private readonly NotificationService _notificationService;
    private readonly PersistenceService _persistenceService;
    private readonly AnalyticsService _analyticsService;
    
    private AppData _appData;
    private SessionLog? _currentSession;
    private int _sessionDistractedSeconds = 0;
    private int _sessionFocusedSeconds = 0;
    private int _continuousDistractedSeconds = 0;
    private DateTime? _lastDistractedReminderTime;
    private string _currentForegroundApp = string.Empty;

    // UI properties
    private string _timerDisplay = "00:00";
    private string _phaseDisplay = "Ready";
    private bool _isSessionActive = false;
    private int _selectedPresetIndex = 1; // Default: 90 min
    private string _currentDomain = "";
    private string _activeTaskTitle = "None";
    private TaskItem? _selectedTask;
    private string _distractionTooltip = "No distractions yet";

    public MainViewModel()
    {
        // Initialize services
        _persistenceService = new PersistenceService();
        _appData = _persistenceService.Load();
        _timerEngine = new TimerEngine();
        _scheduleBuilder = new ScheduleBuilder();
        _foregroundTracker = new ForegroundAppTracker();
        _policyEngine = new DistractionPolicyEngine(_appData.Settings);
        _notificationService = new NotificationService();
        _analyticsService = new AnalyticsService(_appData);

        // Initialize collections
        SessionTasks = new ObservableCollection<TaskItem>();
        SessionPresets = new ObservableCollection<string>
        {
            "10 min", "15 min", "20 min", "30 min", "45 min", "60 min", "90 min", "120 min", 
            "150 min", "180 min", "210 min", "240 min"
        };
        DistractionEvents = new ObservableCollection<DistractionEvent>();

        // Wire up events
        _timerEngine.Tick += OnTimerTick;
        _timerEngine.PhaseChanged += OnPhaseChanged;
        _timerEngine.SessionCompleted += OnSessionCompleted;
        _foregroundTracker.ForegroundAppChanged += OnForegroundAppChanged;

        // Commands
        StartCommand = new RelayCommand(StartSession, () => !_isSessionActive);
        PauseCommand = new RelayCommand(PauseSession, () => _isSessionActive && !_timerEngine.IsPaused);
        ResumeCommand = new RelayCommand(ResumeSession, () => _isSessionActive && _timerEngine.IsPaused);
        ResetCommand = new RelayCommand(ResetSession, () => _isSessionActive);
        SkipBreakCommand = new RelayCommand(SkipBreak, () => _isSessionActive && _phaseDisplay == "BREAK");
        AddTaskCommand = new RelayCommand(AddTask);
        EditTaskCommand = new RelayCommand(EditTask, () => _selectedTask != null);
        DeleteTaskCommand = new RelayCommand(DeleteTask, () => _selectedTask != null);
        OpenHistoryCommand = new RelayCommand(OpenHistory);
        OpenSettingsCommand = new RelayCommand(OpenSettings);

        UpdateStreakDisplay();
    }

    // Properties
    public ObservableCollection<string> SessionPresets { get; }
    public ObservableCollection<TaskItem> SessionTasks { get; }
    public ObservableCollection<DistractionEvent> DistractionEvents { get; }
    public ObservableCollection<BrowserTab>? BrowserTabs { get; set; }
    
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

    public string CurrentDomain
    {
        get => _currentDomain;
        set => SetProperty(ref _currentDomain, value);
    }

    public string ActiveTaskTitle
    {
        get => _activeTaskTitle;
        set => SetProperty(ref _activeTaskTitle, value);
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (SetProperty(ref _selectedTask, value))
            {
                // Trigger CanExecute re-evaluation for edit/delete commands
                (EditTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string FocusedDisplay => $"Focused: {_sessionFocusedSeconds / 60}m";
    public string DistractedDisplay => $"Distracted: {_sessionDistractedSeconds / 60}m";
    public string DistractionTooltip
    {
        get => _distractionTooltip;
        set => SetProperty(ref _distractionTooltip, value);
    }
    public string StreakDisplay { get; private set; } = "Streak: 0 days";

    // Commands
    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SkipBreakCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand EditTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand OpenHistoryCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    private void StartSession()
    {
        // Get selected duration (must match SessionPresets order)
        int[] presetMinutes = { 10, 15, 20, 30, 45, 60, 90, 120, 150, 180, 210, 240 };
        int totalMinutes = presetMinutes[_selectedPresetIndex];

        // Build schedule
        var segments = _scheduleBuilder.Build(totalMinutes);

        // Create session log
        _currentSession = new SessionLog
        {
            StartTimeLocal = DateTime.Now,
            PlannedTotalMinutes = totalMinutes,
            TasksSnapshot = new List<TaskItem>(SessionTasks)
        };

        // Reset tracking
        _sessionFocusedSeconds = 0;
        _sessionDistractedSeconds = 0;
        _continuousDistractedSeconds = 0;
        DistractionEvents.Clear();
        UpdateDistractionTooltip();
        _policyEngine.ResetWorkSegment();

        // Start timer and tracking
        _timerEngine.Start(segments);
        _foregroundTracker.Start();
        IsSessionActive = true;
        
        // Autosave
        SaveData();
    }

    private void PauseSession()
    {
        _timerEngine.Pause();
        _foregroundTracker.Stop();
    }

    private void ResumeSession()
    {
        _timerEngine.Resume();
        _foregroundTracker.Start();
    }

    private void SkipBreak()
    {
        _timerEngine.SkipBreak();
    }

    private void ResetSession()
    {
        // Stop current session
        _timerEngine.Stop();
        
        // Reset to initial state
        _sessionFocusedSeconds = 0;
        _sessionDistractedSeconds = 0;
        _continuousDistractedSeconds = 0;
        _lastDistractedReminderTime = null;
        _currentSession = null;
        DistractionEvents.Clear();
        UpdateDistractionTooltip();
        
        IsSessionActive = false;
        TimerDisplay = "00:00";
        PhaseDisplay = "Ready";
        ActiveTaskTitle = "None";
        
        OnPropertyChanged(nameof(FocusedDisplay));
        OnPropertyChanged(nameof(DistractedDisplay));
        
        _notificationService.ShowNotification("Session Reset", "Session has been reset. Ready to start a new one!");
    }

    public event EventHandler<TaskEditEventArgs>? TaskEditRequested;

    private void AddTask()
    {
        TaskEditRequested?.Invoke(this, new TaskEditEventArgs(null));
    }

    private void EditTask()
    {
        if (_selectedTask != null)
        {
            TaskEditRequested?.Invoke(this, new TaskEditEventArgs(_selectedTask));
        }
    }

    private void DeleteTask()
    {
        if (_selectedTask != null)
        {
            SessionTasks.Remove(_selectedTask);
            SelectedTask = null;
        }
    }

    public event EventHandler? OpenHistoryRequested;
    public event EventHandler? OpenSettingsRequested;

    private void OpenHistory()
    {
        OpenHistoryRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OpenSettings()
    {
        OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public NotificationService NotificationService => _notificationService;
    public AppData AppData => _appData;
    public PersistenceService PersistenceService => _persistenceService;

    private void OnTimerTick(object? sender, EventArgs e)
    {
        // Update timer display
        int seconds = _timerEngine.RemainingSeconds;
        int mins = seconds / 60;
        int secs = seconds % 60;
        TimerDisplay = $"{mins:D2}:{secs:D2}";

        // Track focus/distraction
        bool isWorkPhase = _phaseDisplay == "WORK";

        if (isWorkPhase)
        {
            bool isFocused = false;

            // Check if using FocusTime app (browsing)
            bool isBrowsing = _currentForegroundApp.Contains("FocusTime", StringComparison.OrdinalIgnoreCase);
            
            if (isBrowsing)
            {
                // For browser: check if domain is work-related or blocked
                // Only focus if domain is NOT in blocklist
                bool isDomainBlocked = !string.IsNullOrEmpty(_currentDomain) && 
                                      _appData.Settings.DomainBlocklist.Any(d => 
                                          _currentDomain.Contains(d, StringComparison.OrdinalIgnoreCase));
                
                // Empty domain or local = transitioning, treat as neutral (focused)
                bool isNeutralDomain = string.IsNullOrEmpty(_currentDomain) || 
                                       _currentDomain == "local" || 
                                       _currentDomain == "about:blank";
                
                isFocused = !isDomainBlocked || isNeutralDomain;
            }
            else
            {
                // For other apps: check allowlist
                isFocused = _policyEngine.IsAppAllowed(_currentForegroundApp);
            }

            if (isFocused)
            {
                _sessionFocusedSeconds++;
                _continuousDistractedSeconds = 0;
            }
            else
            {
                _sessionDistractedSeconds++;
                _continuousDistractedSeconds++;

                // Track distraction event (every 60 seconds)
                if (_sessionDistractedSeconds % 60 == 0)
                {
                    var lastEvent = DistractionEvents.LastOrDefault();
                    string source = isBrowsing ? _currentDomain : _currentForegroundApp;
                    
                    if (lastEvent == null || lastEvent.Source != source)
                    {
                        DistractionEvents.Add(new DistractionEvent
                        {
                            Time = DateTime.Now,
                            Source = source,
                            DurationSeconds = 60
                        });
                    }
                    else
                    {
                        lastEvent.DurationSeconds += 60;
                    }
                    UpdateDistractionTooltip();
                }

                // Check for distracted reminder
                if (_continuousDistractedSeconds >= _appData.Settings.DistractedRemindMinutes * 60)
                {
                    if (_lastDistractedReminderTime == null || 
                        (DateTime.Now - _lastDistractedReminderTime.Value).TotalMinutes >= 5)
                    {
                        _notificationService.ShowDistractedReminder(ActiveTaskTitle);
                        _lastDistractedReminderTime = DateTime.Now;
                    }
                }

                // Track app seconds for distraction
                if (_currentSession != null && !string.IsNullOrEmpty(_currentForegroundApp))
                {
                    if (!_currentSession.AppSeconds.ContainsKey(_currentForegroundApp))
                        _currentSession.AppSeconds[_currentForegroundApp] = 0;
                    _currentSession.AppSeconds[_currentForegroundApp]++;
                }
            }
        }

        OnPropertyChanged(nameof(FocusedDisplay));
        OnPropertyChanged(nameof(DistractedDisplay));
    }

    private void OnPhaseChanged(object? sender, PhaseChangedEventArgs e)
    {
        PhaseDisplay = e.NewPhase.ToUpper();
        
        if (e.NewPhase == "Break")
        {
            _notificationService.ShowBreakStart();
            _continuousDistractedSeconds = 0;
        }
        else if (e.NewPhase == "Work")
        {
            _policyEngine.ResetWorkSegment();
        }
    }

    private void OnSessionCompleted(object? sender, EventArgs e)
    {
        // Save session
        if (_currentSession != null)
        {
            _currentSession.EndTimeLocal = DateTime.Now;
            _currentSession.FocusedSeconds = _sessionFocusedSeconds;
            _currentSession.DistractedSeconds = _sessionDistractedSeconds;

            var todayKey = DateKeyHelper.GetToday();
            if (!_appData.Days.ContainsKey(todayKey))
            {
                _appData.Days[todayKey] = new DayLog { DateKey = todayKey };
            }

            _appData.Days[todayKey].Sessions.Add(_currentSession);
            _appData.Days[todayKey].TotalFocusedSeconds += _sessionFocusedSeconds;
            _appData.Days[todayKey].TotalDistractedSeconds += _sessionDistractedSeconds;

            SaveData();
        }

        // Stop tracking
        _foregroundTracker.Stop();
        IsSessionActive = false;
        PhaseDisplay = "Completed";
        
        UpdateStreakDisplay();
    }

    private void OnForegroundAppChanged(object? sender, string processName)
    {
        _currentForegroundApp = processName;
    }

    public void OnDomainChanged(string domain)
    {
        CurrentDomain = domain;

        // Track domain in session
        if (_currentSession != null && !string.IsNullOrEmpty(domain) && domain != "local")
        {
            if (!_currentSession.DomainSeconds.ContainsKey(domain))
                _currentSession.DomainSeconds[domain] = 0;
            _currentSession.DomainSeconds[domain]++;
        }

        // Check if blocked
        bool isWorkPhase = _phaseDisplay == "WORK";
        if (isWorkPhase && _appData.Settings.DomainBlocklist.Any(d => domain.Contains(d)))
        {
            bool shouldBlock = _policyEngine.TrackDomainUsage(domain, true);
            if (shouldBlock)
            {
                _policyEngine.IsDomainBlocked(domain, out DateTime? timeoutUntil);
                if (timeoutUntil.HasValue)
                {
                    OnDomainShouldBeBlocked?.Invoke(this, new DomainBlockedEventArgs
                    {
                        Domain = domain,
                        TimeoutUntil = timeoutUntil.Value
                    });
                }
            }
        }
    }

    public event EventHandler<DomainBlockedEventArgs>? OnDomainShouldBeBlocked;

    private void UpdateStreakDisplay()
    {
        int streak = _analyticsService.CalculateStreak();
        StreakDisplay = $"Streak: {streak} days | Goal: {_appData.Settings.DailyGoalMinutes}m";
        OnPropertyChanged(nameof(StreakDisplay));
    }

    private void UpdateDistractionTooltip()
    {
        if (DistractionEvents.Count == 0)
        {
            DistractionTooltip = "No distractions in this session";
            return;
        }

        var tooltip = new System.Text.StringBuilder();
        tooltip.AppendLine($"Total: {_sessionDistractedSeconds / 60}m {_sessionDistractedSeconds % 60}s\\n");
        tooltip.AppendLine("Distraction breakdown:");
        
        foreach (var evt in DistractionEvents.TakeLast(10))
        {
            int minutes = evt.DurationSeconds / 60;
            int seconds = evt.DurationSeconds % 60;
            string timeStr = evt.Time.ToString("HH:mm:ss");
            tooltip.AppendLine($"• {timeStr} - {evt.Source} ({minutes}m {seconds}s)");
        }

        if (DistractionEvents.Count > 10)
        {
            tooltip.AppendLine($"\\n... and {DistractionEvents.Count - 10} more");
        }

        DistractionTooltip = tooltip.ToString();
    }

    private void SaveData()
    {
        _persistenceService.Save(_appData);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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

/// <summary>
/// Simple relay command implementation
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

public class DomainBlockedEventArgs : EventArgs
{
    public string Domain { get; set; } = string.Empty;
    public DateTime TimeoutUntil { get; set; }
}

public class TaskEditEventArgs : EventArgs
{
    public TaskItem? Task { get; set; }
    
    public TaskEditEventArgs(TaskItem? task)
    {
        Task = task;
    }
}
public class DistractionEvent
{
    public DateTime Time { get; set; }
    public string Source { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
}