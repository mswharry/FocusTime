# Modules Specification

## A) ScheduleBuilder (thuật toán chia Work/Break)
Input: totalMinutes T (45/60/90/120/150/180/210/240)

Profile selection:
- If T <= 60: work=20, break=5
- Else if T <= 120: work=25, break=5
- Else: work=50, break=10

Build cycles:
- remaining = T
- while remaining >= work+break: add (work, break), remaining -= (work+break)
- if remaining > 0:
  - if remaining <= work: add (remaining, 0)
  - else add (work, remaining-work)

Output: list of segments with type Work/Break and minutes.

## B) TimerEngine
- Tick mỗi 1s.
- Auto chuyển phase theo schedule segments.
- Skip Break: nếu đang Break -> set remaining=0 để chuyển segment tiếp theo.
- Events:
  - OnPhaseChanged(Work/Break)
  - OnSegmentCompleted
  - OnSessionCompleted

## C) Session Tasks
- User nhập tasks tùy ý, mỗi task có fields: title, estimateMinutes?, tags[], priority (1-3), subtasks? (optional), note?
- User chọn ActiveTaskId (1 cái).
- Outcome per task: Todo / Doing / Done / Partial / Blocked (MVP có thể dùng: Todo/Doing/Done + end session chọn Partial/Blocked)
- Task breakdown AI: PHASE 2 (không làm).

## D) Focus Browser (WebView2)
- Navigation event lấy URL -> parse domain.
- DomainTime tracking:
  - chỉ tính khi app window focused và phase==Work (mặc định)
  - trong Break: optional (default không tính distracted)
- Blocklist:
  - domains mặc định: facebook.com, youtube.com, tiktok.com, instagram.com, reddit.com (có thể chỉnh)
- Timeout rule (Work phase):
  - nếu domain nằm trong blocklist:
    - cho phép dùng tối đa “allowedSecondsInWorkPerBlock” = 90s (default)
    - vượt ngưỡng -> set timeoutUntil = now + 10 phút
    - trong thời gian timeout, điều hướng vào domain đó sẽ bị redirect sang trang Blocked nội bộ (HTML local) hiển thị countdown/giờ hết hạn
- (MVP) Không cần allowlist domain, chỉ blocklist + timeout.

## E) ForegroundAppTracker (app/process-level)
- Poll mỗi 1s:
  - foreground process name + window title
- Allowlist apps (process names) mặc định:
  - Code.exe, devenv.exe, vmware.exe, vmware-ui.exe, WINWORD.EXE, EXCEL.EXE, POWERPNT.EXE, notepad.exe, obsidian.exe (tùy)
- Rule:
  - nếu phase==Work và foreground app != FocusTime.App:
    - nếu process in allowlist -> không tính distracted (coi là work)
    - else -> tính outside-focus time (distracted) và kích hoạt Prompt/Remind

## F) Distraction Prompt (hỏi mục đích + bao lâu)
Trigger:
- phase==Work, user ở outside-focus app không allowlist
- OR user vào blocklist domain trong Focus Browser (trước khi timeout)

Prompt UI (mini dialog, nhanh):
- “Bạn mở cái này để làm gì?” options: Work / Needed / Break / Lạc
- “Trong bao lâu?” options: 1m / 3m / 5m / 10m
Behavior:
- Nếu Work/Needed: cho phép trong duration đó (grace window) và không tính distracted trong duration (hoặc tính nhẹ tùy bạn; MVP: không tính).
- Nếu Break/Lạc hoặc hết duration: bắt đầu tính distracted.

## G) Distraction scoring + BreakBank rule
- Track sessionFocusedSeconds, sessionDistractedSeconds.
- BreakBank:
  - mỗi khi bước vào Break segment, compute plannedBreakMinutes.
  - discountedBreak = max(0, plannedBreakMinutes - distractedMinutesInPrevWork / 2)
  - break segment remaining = discountedBreak * 60
- Nếu user distracted trong Break: MVP không trừ thêm (đơn giản).

## H) Reminders
- Break start reminder: toast + sound.
- Distracted continuous >= 5 phút trong Work: toast + sound (nhắc quay lại).
- Toast implementation khó -> fallback always-on-top popup + sound.

## I) Daily Goal + Streak + Trend
- Setting: dailyGoalMinutes (default 120)
- Per day: totalFocusedMinutes (sum sessions)
- Achieved = totalFocusedMinutes >= dailyGoalMinutes
- Streak = số ngày liên tiếp tính đến hôm nay thỏa achieved.
- Trend 7 ngày: list (date, focused, distracted, achieved)

## J) Persistence
- JSON file, schemaVersion=1
- Lưu: settings + dayLogs (theo date key YYYY-MM-DD) + sessions list.