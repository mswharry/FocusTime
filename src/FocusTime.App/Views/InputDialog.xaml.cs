using System.Windows;

namespace FocusTime.App.Views;

public partial class InputDialog : Window
{
    public string InputValue { get; private set; } = string.Empty;

    public InputDialog(string prompt, string title = "Input", string defaultValue = "")
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        InputTextBox.Text = defaultValue;
        InputTextBox.Focus();
        InputTextBox.SelectAll();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        InputValue = InputTextBox.Text;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
