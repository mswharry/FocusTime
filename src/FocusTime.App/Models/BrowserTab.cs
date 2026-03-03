using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Web.WebView2.Wpf;

namespace FocusTime.App.Models;

public class BrowserTab : INotifyPropertyChanged
{
    private string _title = "New Tab";
    private string _url = "https://www.google.com";
    private bool _isActive = false;
    private bool _isLoading = false;

    public string TabId { get; set; } = Guid.NewGuid().ToString();
    
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public WebView2? WebView { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
