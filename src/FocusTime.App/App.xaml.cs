using System.Windows;
using System.Diagnostics;

namespace FocusTime.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            
            // Any app-level initialization can go here
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Startup error: {ex.Message}\n{ex.StackTrace}");
            MessageBox.Show($"Startup error: {ex.Message}\n\n{ex.StackTrace}", "FocusTime Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
