using System.Windows;
using FocusTime.Core.Models;

namespace FocusTime.App.Views;

/// <summary>
/// Dialog for session recovery
/// </summary>
public partial class SessionRecoveryDialog : Window
{
    public bool ShouldResume { get; private set; }

    public SessionRecoveryDialog(SessionSnapshot snapshot)
    {
        InitializeComponent();

        // Build session info string
        var elapsed = DateTime.Now - snapshot.SnapshotTime;
        var focusedMins = snapshot.FocusedSeconds / 60;
        var distractedMins = snapshot.DistractedSeconds / 60;
        
        string info = $"Session started: {snapshot.StartTime:g}\n";
        info += $"Planned duration: {snapshot.PlannedTotalMinutes} minutes\n";
        info += $"Current phase: {snapshot.CurrentPhase}\n";
        info += $"Focused: {focusedMins}m | Distracted: {distractedMins}m\n";
        info += $"Last saved: {elapsed.TotalMinutes:F0} minutes ago";

        SessionInfo = info;
        DataContext = this;
    }

    public string SessionInfo { get; }

    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        ShouldResume = true;
        DialogResult = true;
        Close();
    }

    private void Discard_Click(object sender, RoutedEventArgs e)
    {
        ShouldResume = false;
        DialogResult = false;
        Close();
    }
}
