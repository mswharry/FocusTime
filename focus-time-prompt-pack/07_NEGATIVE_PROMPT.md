# Negative Prompt

Không được:
- Thêm backend/login/sync/cloud, telemetry, analytics gửi ra ngoài.
- Thêm extension browser.
- Thêm “điểm phạt” toxic, leaderboard, shame text.
- Thêm quá nhiều màn hình/phức tạp UI.
- Dùng Redux/MVVM framework nặng hoặc microservices.
- Làm proxy/VPN/mitm để bắt domain.
- Làm CDP remote debugging hoặc UI automation đọc address bar của Chrome.
- Bắt user cấu hình phức tạp (cert, flags, registry nhiều bước).
- Tập trung vào đồ họa/animation thay vì core features.

Nếu gặp khó khăn với toast:
- PHẢI fallback popup always-on-top + sound, không được bỏ reminders.