namespace FocusTime.Core.Services;

/// <summary>
/// Handles notifications (toast or popup fallback)
/// Note: Toast implementation is in the App layer due to WinRT dependencies
/// </summary>
public class NotificationService
{
    public event EventHandler<NotificationEventArgs>? NotificationRequested;

    /// <summary>
    /// Show a notification with title and message
    /// </summary>
    public void ShowNotification(string title, string message, bool playSound = true)
    {
        NotificationRequested?.Invoke(this, new NotificationEventArgs
        {
            Title = title,
            Message = message,
            PlaySound = playSound
        });
    }

    /// <summary>
    /// Show break start notification
    /// </summary>
    public void ShowBreakStart()
    {
        ShowNotification("Đến giờ nghỉ rồi", "Uống nước/đứng dậy 1 chút nhé.");
    }

    /// <summary>
    /// Show distracted too long notification
    /// </summary>
    public void ShowDistractedReminder(string taskTitle)
    {
        ShowNotification("Bạn đang lệch hướng hơi lâu", 
            $"Có muốn quay lại task '{taskTitle}' không?");
    }

    /// <summary>
    /// Show domain blocked notification
    /// </summary>
    public void ShowDomainBlocked(string domain, DateTime timeoutUntil)
    {
        string timeStr = timeoutUntil.ToString("HH:mm");
        ShowNotification("Trang đang bị tạm khóa", 
            $"{domain} bị khóa đến {timeStr} để giúp bạn giữ nhịp.");
    }
}

/// <summary>
/// Event args for notification requests
/// </summary>
public class NotificationEventArgs : EventArgs
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool PlaySound { get; set; }
}
