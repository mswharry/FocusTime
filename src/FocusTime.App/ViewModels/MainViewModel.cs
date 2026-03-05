using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using FocusTime.Core.Models;
using FocusTime.Core.Services;
using FocusTime.Core.Helpers;
using FocusTime.App.Models;

namespace FocusTime.App.ViewModels;

/// <summary>
/// Main view model for the main window
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly TimerEngine _timerEngine;
    private readonly ScheduleBuilder _scheduleBuilder;
    private readonly ForegroundAppTracker _foregroundTracker;
    private readonly DistractionPolicyEngine _policyEngine;
    private readonly DistractionPromptService _promptService;
    private readonly NotificationService _notificationService;
    private readonly PersistenceService _persistenceService;
    private readonly AnalyticsService _analyticsService;
    private readonly System.Timers.Timer _autoSaveTimer;
    private bool _disposed = false;
    
    private AppData _appData;
    private SessionLog? _currentSession;
    private List<ScheduleSegment> _currentSegments = new List<ScheduleSegment>();
    private int _sessionDistractedSeconds = 0;
    private int _sessionFocusedSeconds = 0;
    private int _continuousDistractedSeconds = 0;
    private int _continuousFocusedSeconds = 0;
    private DateTime? _lastDistractedReminderTime;
    private string _currentForegroundApp = string.Empty;
    private string _lastPromptedApp = string.Empty;

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
        // Capture UI thread dispatcher
        _dispatcher = Dispatcher.CurrentDispatcher;

        // Initialize services
        _persistenceService = new PersistenceService();
        _appData = _persistenceService.Load();
        _timerEngine = new TimerEngine();
        _scheduleBuilder = new ScheduleBuilder();
        _foregroundTracker = new ForegroundAppTracker();
        _policyEngine = new DistractionPolicyEngine(_appData.Settings);
        _promptService = new DistractionPromptService();
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

        // Initialize auto-save timer (save every 45 seconds)
        _autoSaveTimer = new System.Timers.Timer(45000);
        _autoSaveTimer.Elapsed += OnAutoSaveTick;

        UpdateStreakDisplay();

        // Check for session recovery after UI is ready (use dispatcher)
        _dispatcher.BeginInvoke(new Action(CheckForSessionRecovery), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
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
        _currentSegments = segments;

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
        _continuousFocusedSeconds = 0;
        DistractionEvents.Clear();
        UpdateDistractionTooltip();
        _policyEngine.ResetWorkSegment();
        _promptService.ClearGraceWindows();

        // Start timer and tracking
        _timerEngine.Start(segments);
        _foregroundTracker.Start();
        _autoSaveTimer.Start();
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
        _autoSaveTimer.Stop();
        _foregroundTracker.Stop();
        
        // Reset to initial state
        _sessionFocusedSeconds = 0;
        _sessionDistractedSeconds = 0;
        _continuousDistractedSeconds = 0;
        _continuousFocusedSeconds = 0;
        _lastDistractedReminderTime = null;
        _currentSession = null;
        DistractionEvents.Clear();
        UpdateDistractionTooltip();

        // Clear snapshot
        _appData.LastSessionSnapshot = null;
        SaveData();
        
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

    public event EventHandler<string>? DistractionPromptRequested;

    public void HandleDistractionPromptResult(DistractionPromptResult result)
    {
        if (result == null || string.IsNullOrEmpty(_lastPromptedApp))
            return;

        // Handle based on purpose
        switch (result.Purpose)
        {
            case DistractionPurpose.Work:
            case DistractionPurpose.Needed:
                // Set grace window
                _promptService.SetGraceWindow(_lastPromptedApp, result.DurationMinutes);
                _notificationService.ShowNotification("OK", 
                    $"Bạn có {result.DurationMinutes} phút cho việc này. Quay lại tập trung sau đó nhé!");
                break;

            case DistractionPurpose.Break:
            case DistractionPurpose.Wandering:
                // No grace window, will count as distracted
                // Grace window not set, so distraction counting starts immediately
                break;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _dispatcher.Invoke(() =>
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
                bool shouldPrompt = false;
                string sourceForPrompt = "";

                // Check if using FocusTime app (browsing)
                bool isBrowsing = _currentForegroundApp.Contains("FocusTime", StringComparison.OrdinalIgnoreCase);
                
                if (isBrowsing)
                {
                    // NEW LOGIC: 3-tier domain classification
                    // PRIORITY 1: Check if neutral domain (empty, local, about:blank)
                    bool isNeutralDomain = _policyEngine.IsDomainNeutral(_currentDomain);
                    
                    if (isNeutralDomain)
                    {
                        // Neutral = transitioning, treat as focused (don't penalize)
                        isFocused = true;
                    }
                    else
                    {
                        // PRIORITY 2: Check if in grace window (user approved temporarily)
                        bool inGraceWindow = !string.IsNullOrEmpty(_currentDomain) && 
                                            _promptService.IsInGraceWindow(_currentDomain);
                        
                        if (inGraceWindow)
                        {
                            isFocused = true;
                        }
                        else
                        {
                            // PRIORITY 3: Check if allowlist (work domains)
                            bool isAllowedDomain = _policyEngine.IsDomainAllowed(_currentDomain);
                            
                            if (isAllowedDomain)
                            {
                                isFocused = true;
                            }
                            else
                            {
                                // PRIORITY 4: Check if blocklist (distraction domains)
                                bool isBlockedDomain = !string.IsNullOrEmpty(_currentDomain) && 
                                                      _appData.Settings.DomainBlocklist.Any(d => 
                                                          _currentDomain.Contains(d, StringComparison.OrdinalIgnoreCase));
                                
                                if (isBlockedDomain)
                                {
                                    // Blocked domain = distracted + prompt for permission
                                    isFocused = false;
                                    shouldPrompt = _promptService.ShouldShowPrompt(_currentDomain);
                                    sourceForPrompt = _currentDomain;
                                }
                                else
                                {
                                    // DEFAULT: All other domains = DISTRACTED (default deny)
                                    // This prevents doom scrolling on random websites
                                    isFocused = false;
                                    
                                    // Optionally prompt for unknown domains (can be annoying, so disabled by default)
                                    // shouldPrompt = _promptService.ShouldShowPrompt(_currentDomain);
                                    // sourceForPrompt = _currentDomain;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // For other apps: 3-tier classification
                    // PRIORITY 1: Check if neutral (system utilities)
                    bool isNeutral = _policyEngine.IsAppNeutral(_currentForegroundApp);
                    
                    if (isNeutral)
                    {
                        // Neutral apps = don't count as focused or distracted
                        isFocused = true; // Don't penalize for Calculator, Explorer, etc.
                    }
                    else
                    {
                        // PRIORITY 2: Check if in grace window
                        bool inGraceWindow = _promptService.IsInGraceWindow(_currentForegroundApp);
                        
                        if (inGraceWindow)
                        {
                            isFocused = true;
                        }
                        else
                        {
                            // PRIORITY 3: Check allowlist (productive apps)
                            bool isAllowed = _policyEngine.IsAppAllowed(_currentForegroundApp);
                            
                            isFocused = isAllowed;
                            
                            // DEFAULT: Not allowed = distracted + prompt
                            if (!isAllowed && !string.IsNullOrEmpty(_currentForegroundApp))
                            {
                                shouldPrompt = _promptService.ShouldShowPrompt(_currentForegroundApp);
                                sourceForPrompt = _currentForegroundApp;
                            }
                        }
                    }
                }

                // Trigger distraction prompt
                if (shouldPrompt && !string.IsNullOrEmpty(sourceForPrompt))
                {
                    _promptService.MarkPromptShown(sourceForPrompt);
                    _lastPromptedApp = sourceForPrompt;
                    DistractionPromptRequested?.Invoke(this, sourceForPrompt);
                }

                if (isFocused)
                {
                    _sessionFocusedSeconds++;
                    _continuousFocusedSeconds++;
                    
                    // Reset distraction counter after 2 minutes of continuous focus (not just 30s)
                    if (_continuousFocusedSeconds >= 120) // 2 minutes
                    {
                        _continuousDistractedSeconds = 0;
                        _lastDistractedReminderTime = null; // Also reset reminder timer
                    }
                }
                else
                {
                    _sessionDistractedSeconds++;
                    _continuousDistractedSeconds++;
                    _continuousFocusedSeconds = 0;

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
                            (DateTime.Now - _lastDistractedReminderTime.Value).TotalMinutes >= _appData.Settings.DistractedRemindMinutes)
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
        });
    }

    private void OnPhaseChanged(object? sender, PhaseChangedEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            PhaseDisplay = e.NewPhase.ToUpper();
            
            if (e.NewPhase == "Break")
            {
                // Calculate break bank (penalty for distractions)
                int plannedMinutes = _currentSegments[_timerEngine.CurrentSegmentIndex].Minutes;
                int distractedMinutes = _sessionDistractedSeconds / 60;
                int adjustedMinutes = _policyEngine.ComputeBreakBank(plannedMinutes, distractedMinutes);
                
                if (adjustedMinutes < plannedMinutes)
                {
                    // Apply break bank penalty
                    _timerEngine.AdjustCurrentSegmentTime(adjustedMinutes * 60);
                    
                    int penaltyMinutes = plannedMinutes - adjustedMinutes;
                    _notificationService.ShowNotification("Break Adjusted", 
                        $"Break reduced to {adjustedMinutes}m (penalty: {penaltyMinutes}m for {distractedMinutes}m distraction)");
                }
                else
                {
                    // No penalty - full break
                    _notificationService.ShowBreakStart();
                }
                
                _continuousDistractedSeconds = 0;
            }
            else if (e.NewPhase == "Work")
            {
                _policyEngine.ResetWorkSegment();
                // Grace windows now persist across Work phases (removed ClearGraceWindows)
            }
        });
    }

    private void OnSessionCompleted(object? sender, EventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            // Stop auto-save timer
            _autoSaveTimer.Stop();

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

                // Clear snapshot (session completed successfully)
                _appData.LastSessionSnapshot = null;

                SaveData();
            }

            // Stop tracking
            _foregroundTracker.Stop();
            IsSessionActive = false;
            PhaseDisplay = "Completed";
            
            UpdateStreakDisplay();
        });
    }

    private void OnForegroundAppChanged(object? sender, string processName)
    {
        _dispatcher.Invoke(() =>
        {
            _currentForegroundApp = processName;
        });
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
        int longestStreak = _analyticsService.CalculateLongestStreak();
        StreakDisplay = $"Streak: {streak} days | Longest: {longestStreak} | Goal: {_appData.Settings.DailyGoalMinutes}m";
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
        tooltip.AppendLine($"Total: {_sessionDistractedSeconds / 60}m {_sessionDistractedSeconds % 60}s\n");
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
            tooltip.AppendLine($"\n... and {DistractionEvents.Count - 10} more");
        }

        DistractionTooltip = tooltip.ToString();
    }

    private void SaveData()
    {
        _persistenceService.Save(_appData);
    }

    private void OnAutoSaveTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isSessionActive && _currentSession != null)
        {
            SaveSessionSnapshot();
        }
    }

    private void SaveSessionSnapshot()
    {
        if (_currentSession == null || !_isSessionActive)
            return;

        var snapshot = new SessionSnapshot
        {
            SessionId = _currentSession.SessionId,
            StartTime = _currentSession.StartTimeLocal,
            SnapshotTime = DateTime.Now,
            PlannedTotalMinutes = _currentSession.PlannedTotalMinutes,
            CurrentSegmentIndex = _timerEngine.CurrentSegmentIndex,
            CurrentPhase = _timerEngine.CurrentPhase,
            RemainingSecondsInSegment = _timerEngine.RemainingSeconds,
            FocusedSeconds = _sessionFocusedSeconds,
            DistractedSeconds = _sessionDistractedSeconds,
            ActiveTaskTitle = _activeTaskTitle,
            TasksSnapshot = new List<TaskItem>(SessionTasks),
            DomainSeconds = new Dictionary<string, int>(_currentSession.DomainSeconds),
            AppSeconds = new Dictionary<string, int>(_currentSession.AppSeconds),
            InProgress = true,
            Segments = new List<ScheduleSegment>(_currentSegments),
            ContinuousDistractedSeconds = _continuousDistractedSeconds,
            ContinuousFocusedSeconds = _continuousFocusedSeconds
        };

        _appData.LastSessionSnapshot = snapshot;
        SaveData();
    }

    private void CheckForSessionRecovery()
    {
        if (_appData.LastSessionSnapshot != null && _appData.LastSessionSnapshot.InProgress)
        {
            var snapshot = _appData.LastSessionSnapshot;
            var elapsed = DateTime.Now - snapshot.SnapshotTime;

            // Only offer recovery if snapshot is recent (less than 24 hours old)
            if (elapsed.TotalHours < 24)
            {
                // Raise event to show recovery dialog
                SessionRecoveryAvailable?.Invoke(this, snapshot);
            }
            else
            {
                // Clear old snapshot
                _appData.LastSessionSnapshot = null;
                SaveData();
            }
        }
    }

    public event EventHandler<SessionSnapshot>? SessionRecoveryAvailable;

    public void RestoreSession(SessionSnapshot snapshot)
    {
        try
        {
            // Restore session log
            _currentSession = new SessionLog
            {
                SessionId = snapshot.SessionId,
                StartTimeLocal = snapshot.StartTime,
                PlannedTotalMinutes = snapshot.PlannedTotalMinutes,
                TasksSnapshot = new List<TaskItem>(snapshot.TasksSnapshot),
                DomainSeconds = new Dictionary<string, int>(snapshot.DomainSeconds),
                AppSeconds = new Dictionary<string, int>(snapshot.AppSeconds),
                ActiveTaskTitleAtStart = snapshot.ActiveTaskTitle
            };

            // Restore tracking state
            _sessionFocusedSeconds = snapshot.FocusedSeconds;
            _sessionDistractedSeconds = snapshot.DistractedSeconds;
            _continuousDistractedSeconds = snapshot.ContinuousDistractedSeconds;
            _continuousFocusedSeconds = snapshot.ContinuousFocusedSeconds;
            _currentSegments = snapshot.Segments;

            // Restore tasks
            SessionTasks.Clear();
            foreach (var task in snapshot.TasksSnapshot)
            {
                SessionTasks.Add(task);
            }

            // Update UI state
            ActiveTaskTitle = snapshot.ActiveTaskTitle ?? "None";
            IsSessionActive = true;

            // Reconstruct remaining segments for timer
            var remainingSegments = snapshot.Segments.Skip(snapshot.CurrentSegmentIndex).ToList();
            
            // Adjust first segment's remaining time
            if (remainingSegments.Count > 0)
            {
                var firstSegment = new ScheduleSegment
                {
                    Type = remainingSegments[0].Type,
                    Minutes = snapshot.RemainingSecondsInSegment / 60
                };

                // Replace first segment with adjusted one
                var adjustedSegments = new List<ScheduleSegment> { firstSegment };
                adjustedSegments.AddRange(remainingSegments.Skip(1));

                _timerEngine.Start(adjustedSegments);
                
                // Set exact remaining time for accuracy
                _timerEngine.SetRemainingTime(snapshot.RemainingSecondsInSegment);
            }

            // Start tracking
            _foregroundTracker.Start();
            _autoSaveTimer.Start();

            // Update displays
            OnPropertyChanged(nameof(FocusedDisplay));
            OnPropertyChanged(nameof(DistractedDisplay));

            _notificationService.ShowNotification("Session Restored", "Welcome back! Your session has been restored.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Restore session failed: {ex.Message}");
        }
    }

    public void DiscardSnapshot()
    {
        _appData.LastSessionSnapshot = null;
        SaveData();
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

    public void Dispose()
    {
        if (_disposed) return;

        // Save data before cleanup
        SaveData();

        // Stop and dispose timers
        _autoSaveTimer?.Stop();
        _autoSaveTimer?.Dispose();
        
        // Unsubscribe from timer events
        if (_timerEngine != null)
        {
            _timerEngine.Tick -= OnTimerTick;
            _timerEngine.PhaseChanged -= OnPhaseChanged;
            _timerEngine.SessionCompleted -= OnSessionCompleted;
            _timerEngine.Dispose();
        }

        // Unsubscribe from tracker events
        if (_foregroundTracker != null)
        {
            _foregroundTracker.ForegroundAppChanged -= OnForegroundAppChanged;
            _foregroundTracker.Dispose();
        }

        _disposed = true;
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