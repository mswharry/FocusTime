using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FocusTime.Core.Models;

namespace FocusTime.App.ViewModels;

/// <summary>
/// ViewModel for task management
/// </summary>
public class TasksViewModel : INotifyPropertyChanged
{
    private string _activeTaskTitle = "None";
    private TaskItem? _selectedTask;
    private TaskItem? _activeTask;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? AddTaskRequested;
    public event EventHandler<TaskItem?>? EditTaskRequested;

    public TasksViewModel()
    {
        SessionTasks = new ObservableCollection<TaskItem>();

        AddTaskCommand = new RelayCommand(AddTask);
        EditTaskCommand = new RelayCommand(EditTask, () => _selectedTask != null);
        DeleteTaskCommand = new RelayCommand(DeleteTask, () => _selectedTask != null);
    }

    public ObservableCollection<TaskItem> SessionTasks { get; }

    public string ActiveTaskTitle
    {
        get => _activeTaskTitle;
        set => SetProperty(ref _activeTaskTitle, value);
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (SetProperty(ref _selectedTask, value))
            {
                (EditTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand AddTaskCommand { get; }
    public ICommand EditTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }

    private void AddTask()
    {
        AddTaskRequested?.Invoke(this, EventArgs.Empty);
    }

    private void EditTask()
    {
        if (SelectedTask != null)
        {
            EditTaskRequested?.Invoke(this, SelectedTask);
        }
    }

    private void DeleteTask()
    {
        if (SelectedTask != null && SessionTasks.Contains(SelectedTask))
        {
            SessionTasks.Remove(SelectedTask);
            UpdateActiveTaskDisplay();
        }
    }

    public void AddNewTask(TaskItem task)
    {
        SessionTasks.Add(task);
    }

    public void UpdateTask(TaskItem oldTask, TaskItem newTask)
    {
        int index = SessionTasks.IndexOf(oldTask);
        if (index >= 0)
        {
            SessionTasks[index] = newTask;
        }
        UpdateActiveTaskDisplay();
    }

    public void SetActiveTask(TaskItem? task)
    {
        _activeTask = task;
        UpdateActiveTaskDisplay();
    }

    public TaskItem? GetActiveTask()
    {
        return _activeTask;
    }

    private void UpdateActiveTaskDisplay()
    {
        var activeTask = GetActiveTask();
        ActiveTaskTitle = activeTask?.Title ?? "None";
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
