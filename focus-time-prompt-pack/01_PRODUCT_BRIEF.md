# Product Brief

# Focus Time — Product Brief (MVP)

## Mục tiêu
- Tạo một “workspace” giúp người dùng tập trung cho cả học và làm.
- Giải quyết đúng vấn đề doom scrolling bằng domain-level tracking mà KHÔNG cần extension: dùng Focus Browser tích hợp (WebView2).
- Có nhắc nhở (toast + sound), vẫn hoạt động khi app minimized (chạy nền khi session active).
- Có daily goal + streak (trend/FOMO) nhưng không toxic.

## Đối tượng
- Người học/làm IT/ISec, hay bị kéo sang MXH/giải trí.

## Core Loop
1) Chọn tổng thời gian session (preset): 45/60/90/120/150/180/210/240 phút.
2) App tự chia thành chu kỳ Work/Break.
3) User tạo todo list, chọn task active trong session.
4) Làm việc trong app: dùng Focus Browser mở tài liệu/web.
5) Nếu vào domain blocklist quá lâu → timeout domain 10 phút.
6) Nếu alt-tab sang app ngoài không allowlist → hỏi mục đích + thời lượng; quá giới hạn → tính distracted.
7) Cuối ngày: tổng kết focus/distract, tiến độ task, cập nhật daily goal + streak.

## Must-have (MVP)
- Timer engine: tự chuyển phase, có Skip Break.
- Schedule builder: chọn profile 20/5, 25/5, 50/10 theo tổng thời gian (rule ở Modules Spec).
- Session tasks: user nhập tùy ý, chọn active task. Outcome done/partial/blocked.
- Focus Browser (WebView2): URL bar + back/forward + refresh; tracking domain; blocklist + timeout 10 phút trong Work.
- App-level tracking: theo dõi foreground app/process. Allowlist app (VS Code/VM/Office...) để không tính distracted.
- Distraction handling:
  - Nếu vào domain blocklist quá lâu: timeout 10 phút (mặc định).
  - Nếu ở ngoài app/ngoài allowlist: prompt hỏi “mục đích + bao lâu”; nếu không work/hoặc quá thời gian → log distracted.
  - Quy tắc trừ break: breakBank = plannedBreak - distracted/2 (clamp >= 0).
- Reminders:
  - Định kỳ lúc bắt đầu Break: toast + sound.
  - Nếu distracted liên tục >= 5 phút trong Work: toast + sound.
  - Fallback: always-on-top mini popup + sound.
- Daily goal (phút/ngày) + streak (ngày liên tiếp đạt goal) + trend 7 ngày.
- Local persistence: JSON file + schemaVersion.
- History/Insights: xem theo ngày, top distraction domains/apps.

## Nice-to-have (chỉ làm nếu kịp)
- Export JSON.
- Settings UI chỉnh thresholds (timeout minutes, distracted remind minutes) và blocklist/allowlist.

## Non-goals (không làm trong MVP)
- Không extension browser.
- Không backend/login/sync cloud.
- Không AI breakdown/estimate task.
- Không chặn domain ngoài app.
- Không leaderboard, không “phạt” toxic.

## Definition of Done
- Build chạy được, timer hoạt động, WebView2 hoạt động, blocklist+timeout hoạt động trong Work.
- Allowlist app hoạt động: VS Code/VM/Office không bị tính distracted.
- Remind hoạt động (toast hoặc popup fallback) + sound.
- JSON lưu được + mở lại vẫn còn data.
- Daily goal + streak cập nhật đúng.