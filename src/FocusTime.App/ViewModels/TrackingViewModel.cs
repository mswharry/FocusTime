using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocusTime.App.ViewModels;

/// <summary>
/// ViewModel for distraction tracking
/// </summary>
public class TrackingViewModel : INotifyPropertyChanged
{
    private int _sessionDistractedSeconds = 0;
    private int _sessionFocusedSeconds = 0;
    private int _continuousDistractedSeconds = 0;
    private int _continuousFocusedSeconds = 0;
    private string _distractionTooltip = "No distractions yet";

    public event PropertyChangedEventHandler? PropertyChanged;

    public TrackingViewModel()
    {
        DistractionEvents = new ObservableCollection<DistractionEvent>();
    }

    public ObservableCollection<DistractionEvent> DistractionEvents { get; }

    public int SessionDistractedSeconds
    {
        get => _sessionDistractedSeconds;
        set
        {
            if (SetProperty(ref _sessionDistractedSeconds, value))
            {
                OnPropertyChanged(nameof(DistractedDisplay));
            }
        }
    }

    public int SessionFocusedSeconds
    {
        get => _sessionFocusedSeconds;
        set
        {
            if (SetProperty(ref _sessionFocusedSeconds, value))
            {
                OnPropertyChanged(nameof(FocusedDisplay));
            }
        }
    }

    public int ContinuousDistractedSeconds
    {
        get => _continuousDistractedSeconds;
        set => SetProperty(ref _continuousDistractedSeconds, value);
    }

    public int ContinuousFocusedSeconds
    {
        get => _continuousFocusedSeconds;
        set => SetProperty(ref _continuousFocusedSeconds, value);
    }

    public string FocusedDisplay => $"Focused: {_sessionFocusedSeconds / 60}m";
    public string DistractedDisplay => $"Distracted: {_sessionDistractedSeconds / 60}m";

    public string DistractionTooltip
    {
        get => _distractionTooltip;
        set => SetProperty(ref _distractionTooltip, value);
    }

    public void Reset()
    {
        SessionDistractedSeconds = 0;
        SessionFocusedSeconds = 0;
        ContinuousDistractedSeconds = 0;
        ContinuousFocusedSeconds = 0;
        DistractionEvents.Clear();
        DistractionTooltip = "No distractions yet";
    }

    public void RecordDistraction(string source, int durationSeconds)
    {
        DistractionEvents.Add(new DistractionEvent
        {
            Time = DateTime.Now,
            Source = source,
            DurationSeconds = durationSeconds
        });

        UpdateDistractionTooltip();
    }

    private void UpdateDistractionTooltip()
    {
        if (DistractionEvents.Count == 0)
        {
            DistractionTooltip = "No distractions yet";
            return;
        }

        var grouped = DistractionEvents
            .GroupBy(e => e.Source)
            .Select(g => new { Source = g.Key, TotalMinutes = g.Sum(e => e.DurationSeconds) / 60 })
            .OrderByDescending(x => x.TotalMinutes)
            .Take(3);

        var lines = grouped.Select(x => $"{x.Source}: {x.TotalMinutes}m");
        DistractionTooltip = string.Join("\n", lines);
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
