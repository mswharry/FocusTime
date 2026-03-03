# Nhật ký hình thành ý tưởng “Focus Time”

## 1) Bối cảnh và mục tiêu
Ý định ban đầu là làm một project vibe-coding có giá trị sử dụng thật, ưu tiên trải nghiệm thân thiện và có thể mở rộng. Các mối quan tâm chính xoay quanh: tăng tập trung cho học tập và công việc, đồng thời giải quyết thói quen xao nhãng/doom-scrolling khi đang “focus”.

---

## 2) Ý tưởng khởi đầu: Focus timer + todo
Điểm xuất phát là mô hình quen thuộc: đồng hồ đếm ngược theo chu kỳ work/break, kết hợp todo list để đặt mục tiêu trong phiên làm việc. Tuy nhiên, một vấn đề được xác định rất sớm: timer không đảm bảo người dùng thực sự tập trung, vì vẫn có thể mở mạng xã hội hoặc nội dung giải trí trong lúc timer chạy.

Kết luận: nếu chỉ có timer + todo thì chưa giải quyết “đúng vấn đề”.

---

## 3) Cân nhắc “focus score” và rủi ro UX
Một hướng cải tiến là thêm “focus score” hoặc chấm điểm dựa trên mức xao nhãng. Tuy nhiên, nếu điểm số chỉ mang tính phán xét, trải nghiệm dễ trở nên gượng ép, gây cảm giác tội lỗi hoặc mất động lực. Vì vậy, thống nhất rằng: thống kê/điểm (nếu có) phải đóng vai trò như “tín hiệu để điều chỉnh”, không phải công cụ trừng phạt.

---

## 4) Nút thắt kỹ thuật: cần phân biệt domain để chống doom-scrolling
Để xử lý xao nhãng thực tế, cần theo dõi chính xác các trang/nguồn gây xao nhãng (Facebook/YouTube/TikTok...). Từ đây phát sinh nút thắt:

- Web app thuần không thể theo dõi hành vi ở tab/app khác.
- Desktop app thuần thường chỉ nhận biết mức process (chrome.exe), khó truy xuất domain bên trong trình duyệt bên ngoài.
- Browser extension có thể đọc domain chính xác, nhưng yêu cầu cài thêm extension tạo ma sát và bị coi là “loằng ngoằng” cho người dùng.

Kết luận: muốn domain-level tracking mà không dùng extension thì phải kiểm soát “môi trường duyệt web” trong chính ứng dụng.

---

## 5) Chuyển hướng thiết kế: “Focus Browser” như một workspace
Ý tưởng được chốt lại theo hướng workspace: thay vì cố đo domain trong Chrome/Edge bên ngoài, ứng dụng cung cấp một “Focus Browser” tích hợp (WebView2) để người dùng mở tài liệu/web ngay trong app. Nhờ đó, URL/domain được biết chính xác và có thể áp chính sách nhắc/chặn trực tiếp mà không cần extension.

Đồng thời, để xử lý trường hợp người dùng alt-tab ra ngoài app, bổ sung tracking mức ứng dụng/process, kết hợp allowlist cho các ứng dụng phục vụ công việc (VS Code, VM, MS Office…).

---

## 6) Chốt công nghệ: C# WPF + WebView2 (Windows 10/11)
Sau khi cân nhắc Python, lựa chọn ưu tiên cho MVP nhanh, ổn định, dễ publish trên Windows là:
- C# .NET 8
- WPF
- WebView2 cho Focus Browser
- Lưu dữ liệu local bằng JSON

Lý do: tích hợp WebView2 mượt trên Windows, dễ chạy nền/system tray, dễ thông báo/popup, build/publish self-contained thuận tiện cho người dùng.

---

## 7) Quy tắc vận hành sản phẩm (chốt hành vi)
### Session & Timer
- Người dùng chọn mốc session cố định: 45/60/90/120/150/180/210/240 phút.
- Ứng dụng tự chia thành các chu kỳ work/break phù hợp theo profile:
  - T ≤ 60 → 20/5
  - 60 < T ≤ 120 → 25/5
  - T > 120 → 50/10
- Timer tự động chuyển phase và có tùy chọn skip thời gian nghỉ.

### Tasks
- Todo list do người dùng tạo, số lượng task cho session tùy ý.
- Task fields: title, estimate minutes, tags, priority, subtask, note.
- Chia nhỏ task bằng AI/estimate để phase sau.

### Tracking xao nhãng
- Domain-level tracking áp dụng trong Focus Browser (WebView2).
- Blocklist domain theo mặc định; khi dùng quá lâu trong work, domain bị timeout 5–10 phút (mặc định 10 phút).
- Ngoài browser: tracking theo foreground process; allowlist cho các app làm việc (VS Code/VM/Office…) và có cơ chế gắn tag/allow theo nhu cầu.

### Reminder/Coaching
- Nhắc định kỳ vào lúc break.
- Nhắc thêm nếu xao nhãng liên tục ≥ 5 phút.
- Hiển thị bằng Windows toast notification + sound, kèm fallback popup always-on-top nếu toast khó cấu hình.

### “Phạt mềm” không toxic
- Thời gian xao nhãng trong work được quy đổi sang việc rút ngắn break theo tỉ lệ 1/2 (distracted 4 phút → break giảm 2 phút).

### Daily goal & streak
- Có mục tiêu thời gian tập trung theo ngày (user đặt).
- Streak dựa trên việc đạt daily goal nhiều ngày liên tiếp; trend hiển thị 7 ngày để tạo động lực kiểu FOMO nhưng tránh phán xét.

---

## 8) Chốt scope MVP theo deadline
MVP tập trung vào lõi “đúng vấn đề”:
- Timer + schedule builder + auto phase + skip break
- Session tasks + active task
- Focus Browser WebView2 + domain tracking + blocklist + timeout
- Foreground app tracking + allowlist
- Reminders (toast + sound, fallback popup)
- Daily goal + streak + trend cơ bản
- JSON persistence + history theo ngày

Những phần để phase sau:
- AI breakdown/estimate task
- Domain tracking ngoài app mà không extension
- Charts nâng cao, sync cloud/backend

---

## 9) Kết luận ý tưởng cuối cùng
Sản phẩm được định hình thành một workspace trên Windows kết hợp timer, todo và Focus Browser tích hợp để theo dõi domain xao nhãng, nhắc/chặn theo chính sách, đồng thời hỗ trợ allowlist ứng dụng làm việc ngoài browser. Mục tiêu là tạo trải nghiệm tập trung thực tế, có thống kê và streak để giữ nhịp, nhưng tránh gamification/toxic scoring gây áp lực.