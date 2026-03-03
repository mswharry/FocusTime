# Data Model & Persistence (JSON)

## File location
- %APPDATA%\FocusTime\data.json
- Nếu chưa có folder thì tạo.

## SchemaVersion
- schemaVersion: 1

## Types (gợi ý C# records/classes)

AppData
- int SchemaVersion
- Settings Settings
- Dictionary<string, DayLog> Days  // key = "YYYY-MM-DD"

Settings
- int DailyGoalMinutes (default 120)
- List<string> DomainBlocklist
- int DomainTimeoutMinutes (default 10)
- int BlockedAllowedSecondsInWork (default 90)
- int DistractedRemindMinutes (default 5)
- List<string> AppAllowlistProcessNames
- bool CountBreakBrowsingAsDistracted (default false)

DayLog
- string DateKey ("YYYY-MM-DD")
- List<SessionLog> Sessions
- int TotalFocusedSeconds (computed or stored)
- int TotalDistractedSeconds (computed or stored)

SessionLog
- string SessionId (guid)
- DateTime StartTimeLocal
- DateTime EndTimeLocal
- int PlannedTotalMinutes
- List<SegmentLog> Segments
- List<TaskItem> TasksSnapshot
- string? ActiveTaskTitleAtStart
- Dictionary<string,int> DomainSeconds // domain -> seconds (for Focus Browser)
- Dictionary<string,int> AppSeconds    // process -> seconds (outside/allowlist)
- int FocusedSeconds
- int DistractedSeconds

SegmentLog
- string Type ("Work"|"Break")
- int PlannedSeconds
- int ActualSeconds
- int BreakBankAppliedSeconds  // for break segments

TaskItem
- string TaskId
- string Title
- int? EstimateMinutes
- List<string> Tags
- int Priority (1-3)
- string Status ("Todo"|"Doing"|"Done"|"Partial"|"Blocked")
- List<SubtaskItem>? Subtasks
- string? Note

SubtaskItem
- string SubtaskId
- string Title
- bool Done

## Persistence rules
- Load on app start.
- Save:
  - khi start session, end session
  - mỗi 30s khi session active (debounced) để tránh mất dữ liệu
  - khi thay đổi settings
- Validate:
  - nếu JSON lỗi -> backup file cũ, tạo file mới, hiển thị warning nhẹ.

## DateKey helper
- DateKey = DateTime.Now.ToString("yyyy-MM-dd") theo local time.

## Migration
- If SchemaVersion != 1:
  - MVP: refuse load gracefully + backup + reset (ghi log).