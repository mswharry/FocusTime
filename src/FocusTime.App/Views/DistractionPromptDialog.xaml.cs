using System.Windows;
using System.Windows.Controls;
using FocusTime.Core.Services;

namespace FocusTime.App.Views;

/// <summary>
/// Dialog for distraction prompt
/// </summary>
public partial class DistractionPromptDialog : Window
{
    private DistractionPurpose? _selectedPurpose;

    public DistractionPromptDialog(string appOrDomain)
    {
        InitializeComponent();
        AppNameText.Text = $"Ứng dụng: {appOrDomain}";
    }

    public DistractionPromptResult? Result { get; private set; }

    private void Purpose_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string tag)
            return;

        _selectedPurpose = tag switch
        {
            "Work" => DistractionPurpose.Work,
            "Needed" => DistractionPurpose.Needed,
            "Break" => DistractionPurpose.Break,
            "Wandering" => DistractionPurpose.Wandering,
            _ => DistractionPurpose.Wandering
        };

        // If Work or Needed, show duration selection
        if (_selectedPurpose == DistractionPurpose.Work || _selectedPurpose == DistractionPurpose.Needed)
        {
            DurationPanel.Visibility = Visibility.Visible;
        }
        else
        {
            // For Break or Wandering, close immediately with 0 minutes (no grace)
            Result = new DistractionPromptResult
            {
                Purpose = _selectedPurpose.Value,
                DurationMinutes = 0,
                WasDismissed = false
            };
            DialogResult = true;
            Close();
        }
    }

    private void Duration_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string tag)
            return;

        if (!int.TryParse(tag, out int minutes))
            return;

        Result = new DistractionPromptResult
        {
            Purpose = _selectedPurpose ?? DistractionPurpose.Work,
            DurationMinutes = minutes,
            WasDismissed = false
        };

        DialogResult = true;
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // If closed without selection, treat as dismissed (Wandering)
        if (Result == null)
        {
            Result = new DistractionPromptResult
            {
                Purpose = DistractionPurpose.Wandering,
                DurationMinutes = 0,
                WasDismissed = true
            };
        }
        base.OnClosing(e);
    }
}
