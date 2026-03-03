using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;
using FocusTime.App.ViewModels;
using FocusTime.App.Services;
using FocusTime.App.Models;

namespace FocusTime.App.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly BrowserService _browserService;

    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainViewModel();
        _browserService = new BrowserService();
        
        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Wire up browser events
        _browserService.DomainChanged += (s, domain) => _viewModel.OnDomainChanged(domain);
        _viewModel.OnDomainShouldBeBlocked += OnDomainBlocked;

        // Wire up navigation buttons
        BackButton.Click += (s, e) => _browserService.GoBack();
        ForwardButton.Click += (s, e) => _browserService.GoForward();
        RefreshButton.Click += (s, e) => _browserService.Refresh();
        HomeButton.Click += (s, e) => _browserService.NavigateHome();

        // Wire up window open events
        _viewModel.OpenHistoryRequested += OnOpenHistory;
        _viewModel.OpenSettingsRequested += OnOpenSettings;
        _viewModel.TaskEditRequested += OnTaskEditRequested;

        // Wire up notification service
        _viewModel.NotificationService.NotificationRequested += OnNotificationRequested;

        // Set browser tabs to ViewModel
        _viewModel.BrowserTabs = _browserService.Tabs;

        // Listen to tab collection changes
        _browserService.Tabs.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (BrowserTab tab in e.NewItems)
                {
                    CreateWebViewForTab(tab);
                }
            }
        };

        // Create first tab
        _browserService.CreateNewTab();
    }

    private async void CreateWebViewForTab(BrowserTab tab)
    {
        // Create WebView2
        var webView = new WebView2();
        webView.Tag = tab;
        TabContentArea.Children.Add(webView);
        webView.Visibility = tab.IsActive ? Visibility.Visible : Visibility.Collapsed;

        // Create tab header button
        var tabHeader = CreateTabHeaderButton(tab);
        TabHeadersPanel.Children.Add(tabHeader);

        // Initialize WebView2
        await webView.EnsureCoreWebView2Async();
        _browserService.InitializeTabWebView(tab, webView);

        // Listen to tab property changes
        tab.PropertyChanged += (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(BrowserTab.IsActive))
                {
                    webView.Visibility = tab.IsActive ? Visibility.Visible : Visibility.Collapsed;
                    UpdateTabHeaderStyle(tabHeader, tab);
                    
                    if (tab.IsActive)
                    {
                        UrlTextBox.Text = tab.Url;
                    }
                }
                else if (e.PropertyName == nameof(BrowserTab.Title))
                {
                    UpdateTabHeaderTitle(tabHeader, tab);
                }
            });
        };
    }

    private Border CreateTabHeaderButton(BrowserTab tab)
    {
        var border = new Border
        {
            Tag = tab,
            Padding = new Thickness(12, 6, 12, 6),
            Margin = new Thickness(2, 0, 0, 0),
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 204, 204)),
            BorderThickness = new Thickness(1, 1, 1, 0),
            CornerRadius = new CornerRadius(4, 4, 0, 0),
            Cursor = Cursors.Hand
        };

        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

        var titleText = new TextBlock
        {
            Text = tab.Title,
            MaxWidth = 120,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };

        var closeButton = new Button
        {
            Content = "✕",
            Width = 18,
            Height = 18,
            Padding = new Thickness(0),
            Margin = new Thickness(8, 0, 0, 0),
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontSize = 10,
            Tag = tab,
            ToolTip = "Close Tab"
        };
        closeButton.Click += CloseTab_Click;

        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(closeButton);
        border.Child = stackPanel;

        border.MouseLeftButtonDown += (s, e) =>
        {
            if (s is Border b && b.Tag is BrowserTab t)
            {
                _browserService.SetActiveTab(t);
            }
        };

        UpdateTabHeaderStyle(border, tab);
        return border;
    }

    private void UpdateTabHeaderStyle(Border border, BrowserTab tab)
    {
        border.Background = tab.IsActive ?
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)) :
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 230));

        if (border.Child is StackPanel sp && sp.Children[0] is TextBlock tb)
        {
            tb.FontWeight = tab.IsActive ? FontWeights.SemiBold : FontWeights.Normal;
        }
    }

    private void UpdateTabHeaderTitle(Border border, BrowserTab tab)
    {
        if (border.Child is StackPanel sp && sp.Children[0] is TextBlock tb)
        {
            tb.Text = tab.Title;
        }
    }

    private void OnOpenHistory(object? sender, EventArgs e)
    {
        var historyWindow = new HistoryWindow(_viewModel.AppData);
        historyWindow.Owner = this;
        historyWindow.Show();
    }

    private void OnOpenSettings(object? sender, EventArgs e)
    {
        var settingsWindow = new SettingsWindow(_viewModel.AppData, _viewModel.PersistenceService);
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void OnTaskEditRequested(object? sender, TaskEditEventArgs e)
    {
        var dialog = new TaskEditDialog(e.Task);
        dialog.Owner = this;
        
        if (dialog.ShowDialog() == true && dialog.Task != null)
        {
            if (e.Task == null)
            {
                // New task
                _viewModel.SessionTasks.Add(dialog.Task);
            }
            else
            {
                // Edit existing - properties already updated in place
                // Trigger UI refresh by removing and re-adding
                var index = _viewModel.SessionTasks.IndexOf(e.Task);
                if (index >= 0)
                {
                    _viewModel.SessionTasks[index] = dialog.Task;
                }
            }
        }
    }

    private void OnNotificationRequested(object? sender, Core.Services.NotificationEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var popup = new NotificationPopup(e.Title, e.Message);
            popup.Show();

            // TODO: Play sound if e.PlaySound is true
        });
    }

    private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            string url = UrlTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    _browserService.NavigateToUrl(url);
                    UrlTextBox.BorderBrush = System.Windows.Media.Brushes.Gray; // Reset border
                }
                catch (Exception ex)
                {
                    // Show error feedback
                    UrlTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                    
                    // Fallback to Google search
                    var searchUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
                    _browserService.NavigateToUrl(searchUrl);
                    
                    // Reset border after 2 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (s, args) =>
                    {
                        UrlTextBox.BorderBrush = System.Windows.Media.Brushes.Gray;
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
        }
    }

    private void OnDomainBlocked(object? sender, DomainBlockedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _browserService.ShowBlockedPage(e.Domain, e.TimeoutUntil);
        });
    }

    private void NewTabButton_Click(object sender, RoutedEventArgs e)
    {
        _browserService.CreateNewTab();
    }

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is BrowserTab tab)
        {
            // Find and remove corresponding WebView
            var webView = TabContentArea.Children.OfType<WebView2>()
                .FirstOrDefault(wv => wv.Tag == tab);
            
            if (webView != null)
            {
                TabContentArea.Children.Remove(webView);
                webView.Dispose();
            }

            // Find and remove tab header
            var tabHeader = TabHeadersPanel.Children.OfType<Border>()
                .FirstOrDefault(b => b.Tag == tab);
            
            if (tabHeader != null)
            {
                TabHeadersPanel.Children.Remove(tabHeader);
            }

            _browserService.CloseTab(tab);
        }

        e.Handled = true; // Prevent tab click event
    }
}
