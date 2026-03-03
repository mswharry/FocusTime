using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FocusTime.Core.Models;

namespace FocusTime.App.Views;

public partial class TaskEditDialog : Window
{
    public TaskItem? Task { get; private set; }

    public TaskEditDialog(TaskItem? existingTask = null)
    {
        InitializeComponent();

        if (existingTask != null)
        {
            // Edit mode
            Title = "Edit Task";
            Task = existingTask;
            LoadTaskData(existingTask);
        }
        else
        {
            // Create mode
            Title = "New Task";
            Task = new TaskItem();
        }
    }

    private void LoadTaskData(TaskItem task)
    {
        TitleTextBox.Text = task.Title;
        EstimateTextBox.Text = task.EstimateMinutes?.ToString() ?? "";
        TagsTextBox.Text = string.Join(", ", task.Tags);
        NoteTextBox.Text = task.Note ?? "";

        // Set status
        StatusComboBox.SelectedIndex = task.Status switch
        {
            "Todo" => 0,
            "Doing" => 1,
            "Done" => 2,
            "Partial" => 3,
            "Blocked" => 4,
            _ => 0
        };

        // Set priority
        PriorityComboBox.SelectedIndex = task.Priority - 1;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("Please enter a task title.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Save to Task
        if (Task == null)
            Task = new TaskItem();

        Task.Title = TitleTextBox.Text.Trim();

        // Parse estimate
        if (int.TryParse(EstimateTextBox.Text, out int estimate))
            Task.EstimateMinutes = estimate;
        else
            Task.EstimateMinutes = null;

        // Parse tags
        Task.Tags = TagsTextBox.Text
            .Split(',')
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        Task.Note = string.IsNullOrWhiteSpace(NoteTextBox.Text) ? null : NoteTextBox.Text.Trim();

        // Get status
        Task.Status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content?.ToString() ?? "Todo";

        // Get priority
        var selectedPriorityItem = (ComboBoxItem)PriorityComboBox.SelectedItem;
        Task.Priority = int.Parse(selectedPriorityItem.Tag.ToString() ?? "2");

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
