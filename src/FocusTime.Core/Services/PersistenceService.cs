using FocusTime.Core.Helpers;
using FocusTime.Core.Models;
using System.Text.Json;

namespace FocusTime.Core.Services;

/// <summary>
/// Handles loading and saving app data to JSON
/// </summary>
public class PersistenceService
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersistenceService()
    {
        var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusTime");
        Directory.CreateDirectory(appDataFolder);
        _dataFilePath = Path.Combine(appDataFolder, "data.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Load app data from JSON file
    /// </summary>
    public AppData Load()
    {
        if (!File.Exists(_dataFilePath))
        {
            return CreateDefaultAppData();
        }

        try
        {
            string json = File.ReadAllText(_dataFilePath);
            var appData = JsonSerializer.Deserialize<AppData>(json, _jsonOptions);
            
            if (appData == null || appData.SchemaVersion != 1)
            {
                // Backup and reset
                BackupFile();
                return CreateDefaultAppData();
            }

            return appData;
        }
        catch
        {
            // Backup and reset on error
            BackupFile();
            return CreateDefaultAppData();
        }
    }

    /// <summary>
    /// Save app data to JSON file
    /// </summary>
    public void Save(AppData appData)
    {
        try
        {
            string json = JsonSerializer.Serialize(appData, _jsonOptions);
            File.WriteAllText(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save data: {ex.Message}");
        }
    }

    private void BackupFile()
    {
        if (File.Exists(_dataFilePath))
        {
            string backupPath = _dataFilePath + ".backup." + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(_dataFilePath, backupPath);
        }
    }

    private AppData CreateDefaultAppData()
    {
        var appData = new AppData();
        
        // Add seed data (2 demo days)
        var yesterday = DateTime.Now.AddDays(-1);
        var yesterdayKey = DateKeyHelper.GetDateKey(yesterday);
        
        appData.Days[yesterdayKey] = new DayLog
        {
            DateKey = yesterdayKey,
            TotalFocusedSeconds = 5400, // 90 minutes
            TotalDistractedSeconds = 600, // 10 minutes
            Sessions = new List<SessionLog>
            {
                new SessionLog
                {
                    StartTimeLocal = yesterday.AddHours(9),
                    EndTimeLocal = yesterday.AddHours(10).AddMinutes(40),
                    PlannedTotalMinutes = 90,
                    FocusedSeconds = 5400,
                    DistractedSeconds = 600,
                    TasksSnapshot = new List<TaskItem>
                    {
                        new TaskItem { Title = "Read documentation", Status = "Done" },
                        new TaskItem { Title = "Write code", Status = "Partial" }
                    }
                }
            }
        };

        var todayKey = DateKeyHelper.GetToday();
        appData.Days[todayKey] = new DayLog
        {
            DateKey = todayKey,
            TotalFocusedSeconds = 3000, // 50 minutes
            TotalDistractedSeconds = 300, // 5 minutes
            Sessions = new List<SessionLog>()
        };

        return appData;
    }
}
