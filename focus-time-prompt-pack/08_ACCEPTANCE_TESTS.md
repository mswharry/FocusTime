# Acceptance Tests

## Timer & schedule
1) Chọn session 90 phút -> schedule tạo đúng tổng 90 (profile 25/5).
2) Start -> timer đếm xuống, auto chuyển Work->Break.
3) Skip Break -> chuyển sang Work ngay.
4) End session -> session log được lưu.

## Focus Browser + domain tracking + block
5) Mở github.com trong Focus Browser (Work) -> domain tracked.
6) Mở youtube.com trong Work -> sau khi vượt ngưỡng cho phép (90s) -> bị timeout 10 phút và redirect sang Blocked page hiển thị giờ hết hạn.
7) Trong timeout, thử vào lại youtube.com -> vẫn bị chặn.

## App allowlist tracking
8) Bắt đầu Work, alt-tab sang VS Code (allowlist) 2 phút -> không cộng distracted.
9) Alt-tab sang app không allowlist (vd discord/steam) -> hiện prompt hỏi mục đích + thời lượng.

## Distracted reminder
10) Ở ngoài allowlist >= 5 phút -> nhận notification/popup nhắc quay lại.
11) Bắt đầu Break -> nhận break notification/popup.

## BreakBank rule
12) Trong Work, distracted 4 phút -> break kế tiếp giảm 2 phút (1/2).

## Daily goal + streak
13) Set daily goal 60 phút. Tạo sessions focus đủ 60 -> achieved true.
14) Sang ngày mới (simulate dateKey) -> streak tăng đúng nếu liên tiếp.

## Persistence
15) Thoát app, mở lại -> tasks/settings/history vẫn còn.
16) Export JSON (nếu có) -> file tạo được.

## Performance/UX
17) App vẫn mượt khi tracking mỗi 1s, không lag UI.