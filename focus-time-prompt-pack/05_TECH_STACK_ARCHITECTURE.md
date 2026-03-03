# Tech Stack & Architecture

## Stack
- C# .NET 8
- WPF
- Microsoft.Web.WebView2 (NuGet)
- JSON: System.Text.Json

## Project structure (repo)
- src/FocusTime.App (WPF UI)
- src/FocusTime.Core (logic: timer, tracking, persistence models)
- README.md

## Pattern
- MVVM nhẹ:
  - ViewModels: MainViewModel, SettingsViewModel, HistoryViewModel
  - Services in Core: TimerEngine, ScheduleBuilder, TrackingService, PersistenceService, NotificationService

## Services
1) ScheduleBuilder
2) TimerEngine
3) BrowserService (WebView2 wrapper)
4) ForegroundAppTracker (WinAPI)
5) DistractionPolicyEngine (blocklist/timeout, breakBank)
6) NotificationService
   - TryToast() + fallback PopupWindow
7) PersistenceService (load/save JSON)
8) AnalyticsService (daily goal/streak/trend)

## Windows APIs needed
- Foreground window/process:
  - GetForegroundWindow, GetWindowThreadProcessId, OpenProcess, GetModuleBaseName (or Process.GetProcessById)
- Idle time:
  - GetLastInputInfo
(MVP có thể ưu tiên foreground tracking, idle detection optional nếu kịp.)

## Toast note
- Toast trong WPF cần AppUserModelID + shortcut.
- MVP: implement NotificationService theo 2 tầng:
  - Layer 1: Toast (CommunityToolkit.WinUI.Notifications)
  - Layer 2: AlwaysOnTopPopup (WPF Window topmost) + sound