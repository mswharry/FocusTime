# 🎯 Focus Time

> Ứng dụng Windows desktop giúp bạn tập trung khi học tập và làm việc, với trình duyệt tích hợp, chặn domain và theo dõi thời gian tập trung.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue)](https://github.com/dotnet/wpf)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## ✨ Tính Năng Chính

- **⏱️ Smart Timer**: Tự động chia Work/Break theo thời lượng (profiles: 20/5, 25/5, 50/10)
- **🌐 Focus Browser**: WebView2 tích hợp với tracking và chặn domain
- **👀 Distraction Tracking**: Theo dõi app/domain đang dùng, có allowlist thông minh
- **☕ Break Management**: BreakBank điều chỉnh break dựa trên thời gian distracted
- **📊 Daily Goals & Streaks**: Theo dõi tiến độ với mục tiêu hàng ngày và chuỗi ngày
- **🔔 Notifications**: Nhắc nhở break và cảnh báo distraction
- **🔒 Local-First**: Dữ liệu lưu local JSON, không cần cloud/login

## 📋 Yêu Cầu Hệ Thống

- **OS**: Windows 10/11
- **.NET**: SDK 8.0+ (để build) hoặc Runtime 8.0+ (để chạy)
- **WebView2**: Runtime (thường có sẵn trên Windows 11)
- **RAM**: 4GB+ khuyến nghị
- **Disk**: ~100MB cho app

## 🚀 Hướng Dẫn Cài Đặt & Chạy

### Bước 1: Clone Repository

```bash
git clone https://github.com/mswharry/FocusTime.git
cd FocusTime
```

### Bước 2: Cài .NET 8 SDK (nếu chưa có)

1. Tải về: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Chọn: **.NET 8.0 SDK** (Windows x64)
3. Cài đặt và restart terminal

Kiểm tra:
```powershell
dotnet --version
# Phải hiển thị: 8.0.x hoặc cao hơn
```

### Bước 3: Build & Run

#### Chạy từ Source Code:
```powershell
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run app
dotnet run --project src/FocusTime.App
```

#### Publish Self-Contained Executable:
```powershell
# Publish cho Windows x64 (tạo file .exe độc lập, không cần cài .NET Runtime)
dotnet publish src/FocusTime.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# File .exe sẽ ở:
# src/FocusTime.App/bin/Release/net8.0-windows/win-x64/publish/FocusTime.App.exe
```

> 📝 **Lưu ý**: Lần build đầu có thể mất 2-5 phút do tải WebView2 runtime packages.

## 📖 Hướng Dẫn Sử Dụng

### 1️⃣ Bắt Đầu Session

- Chọn thời lượng tổng session: **45, 60, 90, 120, 150, 180, 210, 240 phút**
- App tự động chia thành Work/Break theo profile:
  - ≤60 min: 20 phút Work, 5 phút Break
  - ≤120 min: 25 phút Work, 5 phút Break
  - >120 min: 50 phút Work, 10 phút Break
- Nhấn **Start** để bắt đầu

### 2️⃣ Quản Lý Tasks

- Click **+ Add Task** để tạo task mới
- Click vào task để chọn làm **Active Task**
- Đánh dấu trạng thái: Todo → Doing → Done/Partial/Blocked

### 3️⃣ Focus Browser

- Dùng trình duyệt tích hợp để mở tài liệu, search Google, đọc docs
- **Blocked domains** (facebook, youtube, tiktok, instagram, reddit):
  - Được phép dùng **90 giây** trong mỗi Work phase
  - Vượt ngưỡng → **Timeout 10 phút**
  - Sẽ redirect sang trang "Blocked" với countdown

### 4️⃣ App Tracking

- App theo dõi foreground window (app nào đang active)
- **Allowlist apps** (VS Code, Office, IDE...): không tính distracted
- Apps khác: tính vào thời gian distracted

### 5️⃣ Break Time

- Nhận thông báo khi đến giờ Break
- Có thể **Skip Break** nếu muốn tiếp tục
- BreakBank: nếu distracted nhiều, break time sẽ giảm

### 6️⃣ History & Settings

- **History**: Xem sessions cũ, top distracting domains/apps
- **Settings**: Chỉnh daily goal, thresholds, xem blocklist/allowlist

## ⚙️ Cấu Hình Mặc Định

| Setting | Value | Mô Tả |
|---------|-------|-------|
| Daily Goal | 120 phút | Mục tiêu tập trung mỗi ngày |
| Blocked Domains | facebook.com, youtube.com, tiktok.com, instagram.com, reddit.com | Các domain bị giới hạn |
| Domain Timeout | 10 phút | Thời gian timeout khi vượt ngưỡng |
| Allowed Seconds | 90s | Thời gian cho phép trên blocked domain mỗi Work phase |
| Distraction Reminder | 5 phút | Nhắc nhở khi distracted liên tục |
| Allowed Apps | Code.exe, devenv.exe, WINWORD.EXE, EXCEL.EXE, POWERPNT.EXE, notepad.exe, obsidian.exe | Apps không tính distracted |

> 💡 **Tip**: Chỉnh settings trong app hoặc edit trực tiếp file `%APPDATA%\FocusTime\data.json`

## 📂 Cấu Trúc Project

```
FocusTime/
├── src/
│   ├── FocusTime.Core/          # Business logic layer
│   │   ├── Models/              # Data models
│   │   ├── Services/            # Core services
│   │   └── Helpers/             # Utilities
│   └── FocusTime.App/           # WPF UI layer
│       ├── Views/               # XAML views
│       ├── ViewModels/          # View models
│       ├── Services/            # UI services
│       └── Resources/           # Assets (HTML, sounds)
├── focus-time-prompt-pack/      # Project specifications
├── FocusTime.sln               # Solution file
└── README.md                   # This file
```

## 💾 Dữ Liệu & Privacy

- **Vị trí**: `%APPDATA%\FocusTime\data.json`
- **Format**: JSON thuần, dễ đọc/backup
- **Privacy**: 100% local, không gửi data ra ngoài
- **Backup**: Tự động backup khi detect JSON lỗi

## 🐛 Troubleshooting

### App không chạy - "No .NET SDKs were found"
→ Cài .NET 8 SDK từ [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

### WebView2 lỗi
→ Cài WebView2 Runtime: [https://go.microsoft.com/fwlink/p/?LinkId=2124703](https://go.microsoft.com/fwlink/p/?LinkId=2124703)

### Notification không hiện
→ App dùng popup fallback, sẽ luôn hoạt động (không cần toast permission)

### App tracking không hoạt động
→ Chạy app với quyền bình thường (không cần admin)

## 🤝 Đóng Góp

Project này được tạo cho mục đích học tập (VibeCode thesis). Mọi đóng góp đều được chào đón:

1. Fork repo
2. Create feature branch: `git checkout -b feature/AmazingFeature`
3. Commit changes: `git commit -m 'Add AmazingFeature'`
4. Push to branch: `git push origin feature/AmazingFeature`
5. Open Pull Request

## 📄 License

MIT License - xem file [LICENSE](LICENSE)

## 🙏 Credits

- Phát triển bởi: [mswharry](https://github.com/mswharry)
- VibeCode thesis project
- Built with: .NET 8, WPF, WebView2

## 📞 Liên Hệ

- GitHub: [@mswharry](https://github.com/mswharry)
- Project Link: [https://github.com/mswharry/FocusTime](https://github.com/mswharry/FocusTime)

---

**⭐ Nếu project hữu ích, hãy cho 1 star nhé!**
