using System.Windows;
using FocusTime.App.ViewModels;
using FocusTime.Core.Models;

namespace FocusTime.App.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow(AppData appData)
    {
        InitializeComponent();
        DataContext = new HistoryViewModel(appData);
    }
}
