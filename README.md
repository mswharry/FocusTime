# 🎯 Focus Time

> Ứng dụng Windows desktop giúp bạn tập trung khi học tập và làm việc, với trình duyệt tích hợp, chặn domain và theo dõi thời gian tập trung.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue)](https://github.com/dotnet/wpf)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## ✨ Tính Năng Chính

### Core Features
- **⏱️ Smart Timer**: Tự động chia Work/Break theo thời lượng với 5 profiles khác nhau
  - Pure focus (≤15 min): Không có break, tập trung thuần túy
  - Short sessions (≤30 min): 10m work / 2m break
  - Standard (≤60 min): 20m work / 5m break
  - Pomodoro (≤120 min): 25m work / 5m break
  - Deep work (>120 min): 50m work / 10m break
- **🎯 Flexible Presets**: 12 preset thời lượng từ 10-240 phút (10, 15, 20, 30, 45, 60, 90, 120, 150, 180, 210, 240)
- **🌐 Focus Browser**: WebView2 tích hợp với multi-tabs, navigation, và tracking real-time
- **👀 Smart Distraction Tracking**: 
  - Theo dõi app/domain với allowlist thông minh
  - Grace period 30s: không reset counter khi focus ngắn
  - Distraction tooltip với breakdown chi tiết (10 events gần nhất)
  - Configurable reminder interval (không còn hardcode)
- **☕ Break Management**: BreakBank điều chỉnh break dựa trên thời gian distracted
- **📊 Daily Goals & Streaks**: Theo dõi tiến độ với mục tiêu hàng ngày và chuỗi ngày
- **🔔 Smart Notifications**: 
  - Break reminders với popup always-on-top
  - Distraction alerts với custom interval
  - Auto-close sau 5 giây
- **⚙️ Interactive Settings UI**: 
  - Add/Remove domains từ blocklist trực tiếp trong app
  - Add/Remove apps từ allowlist với validation
  - Protected defaults không thể xóa
  - Real-time configuration updates
- **🔒 Local-First**: Dữ liệu lưu local JSON, không cần cloud/login, 100% privacy

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

- Chọn thời lượng tổng session: **10, 15, 20, 30, 45, 60, 90, 120, 150, 180, 210, 240 phút**
- App tự động chia thành Work/Break theo 5 profiles thông minh:
  - **≤15 min**: Pure focus (không có break) - dành cho sprint ngắn
  - **≤30 min**: 10 phút Work, 2 phút Break - quick sessions
  - **≤60 min**: 20 phút Work, 5 phút Break - standard sessions
  - **≤120 min**: 25 phút Work, 5 phút Break - Pomodoro style
  - **>120 min**: 50 phút Work, 10 phút Break - deep work mode
- Nhấn **Start** để bắt đầu
- Xem tooltip **Distracted** để theo dõi breakdown chi tiết các distraction

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

### 6️⃣ Settings & Configuration

- **Settings Window**: Click nút Settings để mở cửa sổ cấu hình
- **Domain Blocklist**: 
  - Xem danh sách domains bị chặn
  - **Add**: Thêm domain mới vào blocklist
  - **Remove**: Xóa domain (trừ defaults được bảo vệ)
- **App Allowlist**: 
  - Xem danh sách apps được phép (không tính distracted)
  - **Add**: Thêm process name mới (VD: `chrome.exe`, `firefox.exe`)
  - **Remove**: Xóa app (trừ defaults)
- **Distraction Reminder**: Chỉnh số phút để nhắc khi distracted liên tục
- **Daily Goal**: Điều chỉnh mục tiêu focus mỗi ngày

## ⚙️ Cấu Hình Mặc Định

| Setting | Value | Mô Tả |
|---------|-------|-------|
| Daily Goal | 120 phút | Mục tiêu tập trung mỗi ngày |
| Blocked Domains | facebook.com, youtube.com, tiktok.com, instagram.com, reddit.com | Các domain bị giới hạn (có thể thêm/xóa trong Settings) |
| Domain Timeout | 10 phút | Thời gian timeout khi vượt ngưỡng |
| Allowed Seconds | 90s | Thời gian cho phép trên blocked domain mỗi Work phase |
| Distraction Reminder | 5 phút | Nhắc nhở khi distracted liên tục (configurable) |
| Grace Period | 30 giây | Thời gian focus liên tục cần để reset distraction counter |
| Allowed Apps | Code.exe, devenv.exe, WINWORD.EXE, EXCEL.EXE, POWERPNT.EXE, notepad.exe, obsidian.exe, vmware.exe | Apps không tính distracted (có thể thêm/xóa trong Settings) |

> 💡 **Tip**: Chỉnh settings trong app (Settings window) hoặc edit trực tiếp file `%APPDATA%\FocusTime\data.json`

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

## 🏗️ Kiến Trúc Ứng Dụng

### Layer Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRESENTATION LAYER                       │
│                     (FocusTime.App - WPF/XAML)                  │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │ MainWindow   │  │ Settings     │  │ History      │         │
│  │   .xaml      │  │  Window      │  │  Window      │         │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘         │
│         │                  │                  │                  │
│  ┌──────▼──────────────────▼──────────────────▼───────┐        │
│  │            ViewModels (MVVM Pattern)                │        │
│  │  - MainViewModel: Session & timer orchestration    │        │
│  │  - SettingsViewModel: Configuration management     │        │
│  │  - HistoryViewModel: Analytics & logs display      │        │
│  └────────────────────────┬────────────────────────────┘        │
│                           │                                      │
│  ┌────────────────────────▼───────────────────────────┐         │
│  │         UI Services (Platform-specific)            │         │
│  │  - BrowserService: WebView2 wrapper & navigation   │         │
│  │  - NotificationPopup: Always-on-top alerts         │         │
│  └────────────────────────┬────────────────────────────┘        │
└───────────────────────────┼──────────────────────────────────────┘
                            │
                            │ Dependency Injection
                            │
┌───────────────────────────▼──────────────────────────────────────┐
│                      BUSINESS LOGIC LAYER                         │
│                   (FocusTime.Core - Pure C#)                     │
├─────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Core Services                            │ │
│  │  ┌───────────────┐  ┌────────────────┐  ┌───────────────┐ │ │
│  │  │ TimerEngine   │  │ScheduleBuilder │  │AnalyticsService│ │ │
│  │  │ - Tick (1s)   │  │ - Work/Break   │  │ - Sessions    │ │ │
│  │  │ - Phases      │  │ - 5 Profiles   │  │ - Statistics  │ │ │
│  │  └───────────────┘  └────────────────┘  └───────────────┘ │ │
│  │                                                              │ │
│  │  ┌───────────────────┐  ┌──────────────────────────────┐  │ │
│  │  │ ForegroundApp     │  │ DistractionPolicyEngine      │  │ │
│  │  │ Tracker           │  │ - Domain blocklist check     │  │ │
│  │  │ - Win32 API       │  │ - App allowlist validation   │  │ │
│  │  │ - 1s polling      │  │ - Grace period (30s)         │  │ │
│  │  └───────────────────┘  └──────────────────────────────┘  │ │
│  │                                                              │ │
│  │  ┌───────────────────┐  ┌──────────────────────────────┐  │ │
│  │  │ NotificationSvc   │  │ PersistenceService           │  │ │
│  │  │ - Event-based     │  │ - JSON serialization         │  │ │
│  │  │ - Popup triggers  │  │ - Auto-backup on error       │  │ │
│  │  └───────────────────┘  └──────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                      Domain Models                          │ │
│  │  - AppData: Root data container                             │ │
│  │  - Settings: User preferences                               │ │
│  │  - SessionLog: Session history records                      │ │
│  │  - TaskItem: Task management                                │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ File I/O
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         DATA LAYER                               │
│                  %APPDATA%\FocusTime\data.json                  │
└─────────────────────────────────────────────────────────────────┘
```

### Key Design Patterns

1. **MVVM (Model-View-ViewModel)**
   - Clean separation: Views (XAML) ↔ ViewModels ↔ Models
   - Data binding cho reactive UI updates
   - Commands pattern cho user interactions

2. **Observer Pattern**
   - `TimerEngine.Tick` event → MainViewModel subscribes
   - `ForegroundAppTracker.ForegroundAppChanged` → Tracking logic
   - `NotificationService.NotificationRequested` → Popup display

3. **Strategy Pattern**
   - `ScheduleBuilder`: 5 profiles dựa trên session length
   - `DistractionPolicyEngine`: Domain vs App detection strategies

4. **Dependency Injection**
   - Services injected vào ViewModels
   - Loose coupling giữa layers
   - Easy testing & mocking

### Data Flow Example: Session Lifecycle

```
User clicks Start
    │
    ▼
MainViewModel.StartSession()
    │
    ├──▶ ScheduleBuilder.Build(duration)
    │       └──▶ Returns List<Phase> (Work/Break schedule)
    │
    ├──▶ TimerEngine.Start(schedule)
    │       └──▶ Starts 1-second timer
    │
    └──▶ ForegroundAppTracker.Start()
            └──▶ Polls active window every 1s
    
Every second:
    │
    ├──▶ TimerEngine.Tick event
    │       └──▶ MainViewModel: Update timer display
    │
    └──▶ ForegroundAppTracker.ForegroundAppChanged
            │
            ▼
        MainViewModel.OnTimerTick()
            │
            ├──▶ Check if focused (DistractionPolicyEngine)
            │       ├─ Browser? → Domain blocklist check
            │       └─ Other app? → Allowlist check
            │
            ├──▶ Update counters (_sessionFocusedSeconds, _continuousDistractedSeconds)
            │
            ├──▶ Grace period logic (30s continuous focus)
            │
            └──▶ Check distraction reminder threshold
                    └──▶ NotificationService.ShowDistractedReminder()
                            └──▶ NotificationPopup.Show()

Session completes:
    │
    ▼
TimerEngine.SessionCompleted event
    │
    └──▶ MainViewModel: Save SessionLog
            └──▶ PersistenceService.Save(AppData)
                    └──▶ Write to %APPDATA%\FocusTime\data.json
```

### Technology Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| UI Framework | WPF (Windows Presentation Foundation) | Desktop UI with XAML |
| Browser Engine | WebView2 (Chromium) | Embedded browser tabs |
| Win32 API | `GetForegroundWindow()` | Detect active application |
| Timer | `System.Timers.Timer` | 1-second tick engine |
| Data Storage | JSON + `System.Text.Json` | Local persistence |
| Architecture | MVVM + Services | Clean separation of concerns |
| Target Framework | .NET 8.0 | Modern C# features |

## 💾 Dữ Liệu & Privacy

- **Vị trí**: `%APPDATA%\FocusTime\data.json`
- **Format**: JSON thuần, dễ đọc/backup
- **Privacy**: 100% local, không gửi data ra ngoài
- **Backup**: Tự động backup khi detect JSON lỗi

## 🆕 Recent Updates & Enhancements

### v1.2.0 - Smart Distraction Tracking (March 2026)
**Features:**
- ✅ **Grace Period Tracking**: Counter distraction chỉ reset sau 30 giây focus liên tục (không còn reset ngay lập tức)
- ✅ **Configurable Reminders**: Fix hardcoded 5-minute interval → theo setting user
- ✅ **Distraction Tooltip**: Hover để xem breakdown chi tiết 10 distraction events gần nhất
- ✅ **Short Session Presets**: Thêm 10, 15, 20, 30 phút cho quick focus sprints
- ✅ **Dynamic Schedule Profiles**: 5 work/break profiles từ pure-focus đến deep-work mode
- ✅ **Interactive Settings UI**: 
  - Add/Remove domains & apps trực tiếp trong Settings window
  - Protected defaults (không thể xóa các domain/app quan trọng)
  - Real-time validation và feedback

**Bug Fixes:**
- 🐛 Fixed: Timer hiển thị sai duration cho preset mới
- 🐛 Fixed: Distraction reminder hardcoded interval
- 🐛 Fixed: Counter reset quá nhanh khi focus ngắn

**Performance:**
- ⚡ Optimized: Distraction event tracking (chỉ log mỗi 60s thay vì mỗi giây)
- ⚡ Improved: Tooltip generation với caching

### v1.1.0 - Foundation Release
- Initial production-ready MVP
- Core timer engine với break scheduling
- Browser integration với WebView2
- Local JSON persistence
- Distraction tracking cơ bản

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

Project này được phát triển trong khuôn khổ **VibeCode thesis** - nghiên cứu về AI-assisted software development. 

VibeCode là phương pháp phát triển phần mềm kết hợp:
- 🤖 AI Tools (GPT, GitHub Copilot) cho brainstorming và code generation
- 👨‍💻 Human expertise cho architecture decisions và quality control
- ⚡ Rapid iteration với AI feedback loops

Mọi đóng góp đều được chào đón:

1. Fork repo
2. Create feature branch: `git checkout -b feature/AmazingFeature`
3. Commit changes: `git commit -m 'Add AmazingFeature'`
4. Push to branch: `git push origin feature/AmazingFeature`
5. Open Pull Request

## 📄 License

MIT License - xem file [LICENSE](LICENSE)

## 🙏 Credits

### Development
- Phát triển bởi: [mswharry](https://github.com/mswharry)
- VibeCode thesis project
- Built with: .NET 8, WPF, WebView2

### VibeCode - AI-Assisted Development Tools

Project này được phát triển với sự hỗ trợ của VibeCode methodology và các công cụ AI:

**🎨 Ideation & Planning**
- **ChatGPT (GPT-4)**: Brainstorming ý tưởng, hỗ trợ thiết kế architecture, design pattern selection
- Feature planning và requirements analysis
- Technical documentation structure

**💻 Code Generation & Development**
- **GitHub Copilot** (Claude Sonnet 4.5): Primary coding assistant
  - Code generation và completion
  - Refactoring và optimization suggestions
  - Bug detection và fixes
  - XAML/C# best practices
  - Real-time code improvements

**🔄 Development Workflow**
1. **Idea Phase**: GPT-4 brainstorming → Architecture design
2. **Implementation**: GitHub Copilot (Claude Sonnet 4.5) → Code generation
3. **Iteration**: Continuous refinement với AI feedback
4. **Testing**: Manual testing + AI-suggested test cases

> 💡 **VibeCode Philosophy**: Kết hợp sức mạnh của AI tools với human creativity để tạo ra sản phẩm chất lượng cao trong thời gian ngắn.

## 📞 Liên Hệ

- GitHub: [@mswharry](https://github.com/mswharry)
- Project Link: [https://github.com/mswharry/FocusTime](https://github.com/mswharry/FocusTime)

---

**⭐ Nếu project hữu ích, hãy cho 1 star nhé!**
