# UX/UI Specification

# UX/UI Spec (Clean minimal)

## Layout tổng thể (1 window chính)
- Top bar: Session preset dropdown + Start/Pause + Phase (Work/Break) + Remaining time + Skip Break (chỉ khi đang Break).
- Left panel (Tasks):
  - Session Tasks list (title, estimate, tags, priority indicator nhẹ).
  - Button: + Add Task
  - Button: Set Active Task
  - Quick status: Done/Partial/Blocked (per task)
- Center (Focus Browser):
  - Nav bar: Back / Forward / Refresh / Home
  - URL/Search bar
  - WebView2 content
- Bottom status bar:
  - Current domain (nếu đang trong browser)
  - Focused minutes (session)
  - Distracted minutes (session)
  - BreakBank (phút break còn lại nếu đang Break hoặc sắp tới)

## Screens / Pages
### 1) Dashboard (Main)
- Timer + tasks + browser (như layout trên)
- “Session Summary” mini card: focused/distracted + active task + next break time.

### 2) Insights/History (secondary window hoặc tab)
- List ngày gần nhất (7–30).
- Click ngày: show
  - total focused/distracted
  - tasks outcomes
  - top distracting domains/apps
- Trend 7 ngày (text + đơn giản; chart optional nếu kịp).

### 3) Settings (secondary)
- Daily goal minutes input
- Blocklist editor (domain list)
- Domain timeout minutes (default 10)
- Distracted remind threshold minutes (default 5)
- Allowlist apps (process names)
- Toggle: “Count break browsing as distracted?” (default OFF)
- Export JSON button (optional)

## Copywriting
- Thân thiện, trung tính, không phán xét.
- Ví dụ:
  - Break start: “Đến giờ nghỉ rồi. Uống nước/đứng dậy 1 chút nhé.”
  - Distracted too long: “Bạn đang lệch hướng hơi lâu. Có muốn quay lại task ‘X’ không?”
  - Domain blocked: “Trang này đang bị tạm khóa đến HH:MM để giúp bạn giữ nhịp.”

## Interaction rules
- Start session: tạo Session object, start timer.
- Pause: dừng timer + tracking.
- Skip break: chuyển sang Work ngay.
- Active task: 1 task active tại 1 thời điểm.
- Mọi thao tác thường dùng <= 2 click.