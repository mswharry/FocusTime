namespace FocusTime.Core.Helpers;

/// <summary>
/// Helper for generating date keys in YYYY-MM-DD format
/// </summary>
public static class DateKeyHelper
{
    public static string GetToday()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    public static string GetDateKey(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    public static DateTime ParseDateKey(string dateKey)
    {
        return DateTime.ParseExact(dateKey, "yyyy-MM-dd", null);
    }
}
