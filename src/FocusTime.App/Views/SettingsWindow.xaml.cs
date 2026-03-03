using System.Windows;
using FocusTime.App.ViewModels;
using FocusTime.Core.Models;
using FocusTime.Core.Services;

namespace FocusTime.App.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    private readonly PersistenceService _persistenceService;
    private readonly AppData _appData;

    public SettingsWindow(AppData appData, PersistenceService persistenceService)
    {
        InitializeComponent();
        
        _appData = appData;
        _persistenceService = persistenceService;
        _viewModel = new SettingsViewModel(appData.Settings);
        
        DataContext = _viewModel;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveToSettings();
        _persistenceService.Save(_appData);
        
        MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void AddDomain_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog(
            "Enter domain to block (e.g., twitter.com):", 
            "Add Domain");
        dialog.Owner = this;
        
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
        {
            _viewModel.AddDomain(dialog.InputValue.Trim());
        }
    }

    private void RemoveDomain_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedDomain != null)
        {
            if (!_viewModel.CanRemoveDomain(_viewModel.SelectedDomain))
            {
                MessageBox.Show("Cannot remove default domains.", "Remove Domain", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Remove '{_viewModel.SelectedDomain}' from blocklist?", 
                "Confirm Remove", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.RemoveDomain(_viewModel.SelectedDomain);
            }
        }
    }

    private void AddApp_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog(
            "Enter app process name (e.g., Chrome.exe):", 
            "Add App");
        dialog.Owner = this;
        
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
        {
            _viewModel.AddApp(dialog.InputValue.Trim());
        }
    }

    private void RemoveApp_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedApp != null)
        {
            if (!_viewModel.CanRemoveApp(_viewModel.SelectedApp))
            {
                MessageBox.Show("Cannot remove default apps.", "Remove App", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Remove '{_viewModel.SelectedApp}' from allowlist?", 
                "Confirm Remove", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.RemoveApp(_viewModel.SelectedApp);
            }
        }
    }
}
