using System.Windows;
using System.Windows.Threading;

namespace FocusTime.App.Views;

public partial class NotificationPopup : Window
{
    private DispatcherTimer? _autoCloseTimer;

    public NotificationPopup(string title, string message, int autoCloseSeconds = 5)
    {
        InitializeComponent();
        
        TitleText.Text = title;
        MessageText.Text = message;

        // Position at bottom-right of screen
        var workingArea = SystemParameters.WorkArea;
        Left = workingArea.Right - Width - 20;
        Top = workingArea.Bottom - Height - 20;

        // Setup auto-close timer
        _autoCloseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(autoCloseSeconds)
        };
        _autoCloseTimer.Tick += (s, e) => Close();
        _autoCloseTimer.Start();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _autoCloseTimer?.Stop();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _autoCloseTimer?.Stop();
        base.OnClosed(e);
    }
}
