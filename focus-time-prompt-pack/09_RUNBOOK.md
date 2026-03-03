# Runbook

## 1: Setup
- Create repo + solution structure (App + Core)
- Add WebView2 package
- Skeleton MainWindow layout

## 2: Timer core
- ScheduleBuilder + TimerEngine
- UI bind: Start/Pause, Skip Break, show phase/time

## 3: Focus Browser
- WebView2 embed + URL bar
- Domain tracking per second
- Blocklist + timeout + Blocked page

## 4: Foreground tracking + policy
- ForegroundAppTracker + allowlist
- DistractionPrompt (dialog) + distracted continuous logic
- BreakBank deduction

## 5: Persistence + History/Settings
- JSON load/save
- Settings page minimal
- History list by day + simple trend

## 6: Notifications + Polish + Bugfix
- NotificationService toast attempt + fallback popup + sound
- Fix edge cases, run Acceptance tests
- Publish command + GitHub README