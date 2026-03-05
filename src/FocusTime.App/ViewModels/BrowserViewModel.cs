using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FocusTime.App.Models;

namespace FocusTime.App.ViewModels;

/// <summary>
/// ViewModel for browser management
/// </summary>
public class BrowserViewModel : INotifyPropertyChanged
{
    private string _currentDomain = "";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<string>? DomainChanged;

    public BrowserViewModel()
    {
    }

    public ObservableCollection<BrowserTab>? BrowserTabs { get; set; }

    public string CurrentDomain
    {
        get => _currentDomain;
        set
        {
            if (SetProperty(ref _currentDomain, value))
            {
                DomainChanged?.Invoke(this, value);
            }
        }
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
