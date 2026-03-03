using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using FocusTime.App.Models;

namespace FocusTime.App.Services;

/// <summary>
/// Service wrapper for WebView2 browser functionality with multi-tab support
/// </summary>
public class BrowserService
{
    private string _blockedPagePath = string.Empty;

    public ObservableCollection<BrowserTab> Tabs { get; } = new ObservableCollection<BrowserTab>();
    public BrowserTab? ActiveTab { get; private set; }

    public event EventHandler<string>? DomainChanged;
    public string CurrentDomain => ActiveTab != null ? ExtractDomain(ActiveTab.Url) : string.Empty;

    public BrowserService()
    {
        // Find blocked page path
        _blockedPagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "BlockedPage.html");
    }

    public BrowserTab CreateNewTab(string? initialUrl = null)
    {
        var tab = new BrowserTab
        {
            Title = "New Tab",
            Url = initialUrl ?? "https://www.google.com"
        };

        Tabs.Add(tab);
        SetActiveTab(tab);
        return tab;
    }

    public void CloseTab(BrowserTab tab)
    {
        if (Tabs.Count <= 1)
            return; // Don't close last tab

        var index = Tabs.IndexOf(tab);
        Tabs.Remove(tab);

        // Dispose WebView2 if exists
        tab.WebView?.Dispose();

        // Set new active tab
        if (tab == ActiveTab)
        {
            var newIndex = Math.Min(index, Tabs.Count - 1);
            if (newIndex >= 0 && newIndex < Tabs.Count)
                SetActiveTab(Tabs[newIndex]);
        }
    }

    public void SetActiveTab(BrowserTab tab)
    {
        if (ActiveTab != null)
            ActiveTab.IsActive = false;

        ActiveTab = tab;
        tab.IsActive = true;

        // Notify domain change
        DomainChanged?.Invoke(this, CurrentDomain);
    }

    public void InitializeTabWebView(BrowserTab tab, WebView2 webView)
    {
        tab.WebView = webView;
        webView.NavigationStarting += (s, e) => tab.IsLoading = true;
        webView.NavigationCompleted += (s, e) =>
        {
            tab.IsLoading = false;
            
            // Check if navigation failed
            if (!e.IsSuccess)
            {
                // Navigation failed - show error page or retry with search
                var failedUrl = tab.Url;
                if (!failedUrl.Contains("google.com/search"))
                {
                    // Retry as Google search
                    var searchUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(failedUrl);
                    webView.Source = new Uri(searchUrl);
                }
            }
            else
            {
                OnNavigationCompleted(tab, e);
            }
        };

        // Navigate to initial URL
        NavigateToUrl(tab, tab.Url);
    }

    public void NavigateToUrl(BrowserTab tab, string url)
    {
        if (tab.WebView == null) return;

        url = url.Trim();
        
        // Check if it's a valid URL or a search query
        string navigateUrl;
        
        if (IsValidUrl(url))
        {
            // Valid URL - add protocol if needed
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                navigateUrl = "https://" + url;
            }
            else
            {
                navigateUrl = url;
            }
        }
        else
        {
            // Not a valid URL - treat as search query
            navigateUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
        }

        try
        {
            tab.Url = navigateUrl;
            tab.WebView.Source = new Uri(navigateUrl);
        }
        catch (UriFormatException)
        {
            // If still fails, fallback to Google search
            var searchUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
            tab.Url = searchUrl;
            tab.WebView.Source = new Uri(searchUrl);
        }
    }

    private bool IsValidUrl(string url)
    {
        // Check if it looks like a URL (contains domain-like pattern)
        // Valid URLs: google.com, www.google.com, github.com/user/repo, localhost:3000
        // Not valid: facebook (single word), hello world (multiple words)
        
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // If it has spaces, it's a search query
        if (url.Contains(' '))
            return false;

        // If starts with protocol, check if valid URI
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        // Check if it has domain pattern: contains dot or is localhost with port
        if (url.Contains('.') || url.StartsWith("localhost"))
        {
            // Try to validate as URL with https prefix
            return Uri.TryCreate("https://" + url, UriKind.Absolute, out var uri) 
                   && (uri.Host.Contains('.') || uri.Host == "localhost");
        }

        // Single word without dot - treat as search query
        return false;
    }

    public void NavigateToUrl(string url)
    {
        if (ActiveTab != null)
            NavigateToUrl(ActiveTab, url);
    }

    public void NavigateHome()
    {
        NavigateToUrl("https://www.google.com");
    }

    public void GoBack()
    {
        if (ActiveTab?.WebView?.CanGoBack == true)
            ActiveTab.WebView.GoBack();
    }

    public void GoForward()
    {
        if (ActiveTab?.WebView?.CanGoForward == true)
            ActiveTab.WebView.GoForward();
    }

    public void Refresh()
    {
        ActiveTab?.WebView?.Reload();
    }

    /// <summary>
    /// Show blocked page with timeout information
    /// </summary>
    public void ShowBlockedPage(string domain, DateTime timeoutUntil)
    {
        if (ActiveTab?.WebView == null || !File.Exists(_blockedPagePath))
            return;

        string timeStr = timeoutUntil.ToString("HH:mm");
        string url = $"file:///{_blockedPagePath.Replace("\\", "/")}?until={timeStr}";
        ActiveTab.WebView.Source = new Uri(url);
    }

    /// <summary>
    /// Check if current URL matches a domain (with subdomains)
    /// </summary>
    public bool IsCurrentDomain(string domain)
    {
        return CurrentDomain.EndsWith(domain);
    }

    private void OnNavigationCompleted(BrowserTab tab, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        if (tab.WebView?.Source != null)
        {
            string url = tab.WebView.Source.ToString();
            tab.Url = url;

            // Update title
            if (!string.IsNullOrEmpty(tab.WebView.CoreWebView2?.DocumentTitle))
                tab.Title = tab.WebView.CoreWebView2.DocumentTitle;
            else
                tab.Title = ExtractDomain(url);

            // Notify domain change if this is the active tab
            if (tab == ActiveTab)
            {
                DomainChanged?.Invoke(this, CurrentDomain);
            }
        }
    }

    private string ExtractDomain(string url)
    {
        try
        {
            if (url.StartsWith("file://"))
                return "local";

            var uri = new Uri(url);
            return uri.Host.ToLower();
        }
        catch
        {
            return string.Empty;
        }
    }
}
