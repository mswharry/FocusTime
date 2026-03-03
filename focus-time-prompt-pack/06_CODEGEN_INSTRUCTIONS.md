# Code Generation Instructions

## Output format
- Trả về:
  1) File tree đầy đủ
  2) Nội dung từng file chính (manifest không có vì đây là app)
  3) Lệnh build/run/publish

## Yêu cầu code
- Chạy được ngay sau khi `dotnet restore` + `dotnet run`.
- Không over-engineer.
- Có comment vừa đủ.
- Các giá trị default đúng spec.
- Có seed data (2 ngày log mẫu) để demo History/Trend.

## MVP priorities (bắt buộc theo thứ tự)
1) TimerEngine + ScheduleBuilder
2) MainWindow layout + binding tối thiểu (timer, start/pause, skip break)
3) WebView2 embed + URL bar + domain tracking + block page
4) Foreground app tracking + allowlist
5) Distraction policy + timeout + breakBank
6) Persistence JSON
7) History/Settings screens basic
8) NotificationService (toast + fallback)

## Build/publish
- Provide:
  - run from source
  - publish self-contained win-x64

## Không được làm
- Không thêm cloud/login.
- Không thêm AI API.
- Không thêm extension.
- Không dùng thư viện UI quá nặng.