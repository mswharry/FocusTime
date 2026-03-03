# 🔨 Build Guide - Focus Time

Hướng dẫn chi tiết để build và deploy ứng dụng Focus Time.

## 📋 Mục Lục

1. [Prerequisites](#prerequisites)
2. [Build từ Source](#build-từ-source)
3. [Publish Executable](#publish-executable)
4. [Testing](#testing)
5. [Common Issues](#common-issues)

---

## Prerequisites

### 1. .NET 8 SDK

**Cài đặt:**

1. Truy cập: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Tải **".NET 8.0 SDK"** cho Windows x64
3. Chạy installer và làm theo hướng dẫn
4. Restart terminal/PowerShell sau khi cài

**Kiểm tra:**

```powershell
dotnet --version
# Expected output: 8.0.x
```

### 2. Git (Optional)

Để clone repository:

```powershell
git --version
# Nếu chưa có, tải từ: https://git-scm.com/download/win
```

### 3. IDE/Editor (Optional)

Khuyến nghị một trong các tool sau:
- **Visual Studio 2022** (Community/Professional/Enterprise)
- **Visual Studio Code** với C# extension
- **JetBrains Rider**

---

## Build từ Source

### Bước 1: Clone Repository

```powershell
git clone https://github.com/mswharry/FocusTime.git
cd FocusTime
```

Hoặc download ZIP từ GitHub và giải nén.

### Bước 2: Restore NuGet Packages

```powershell
dotnet restore
```

**Output mong đợi:**
```
Determining projects to restore...
Restored FocusTime.Core.csproj (in XX seconds)
Restored FocusTime.App.csproj (in XX seconds)
```

**NuGet packages được tải:**
- `Microsoft.Web.WebView2` - WebView2 browser control
- `Microsoft.Toolkit.Uwp.Notifications` - Toast notifications (fallback: popup)

### Bước 3: Build Solution

```powershell
# Debug build
dotnet build

# Release build (optimized)
dotnet build -c Release
```

**Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Build artifacts:**
- Debug: `src/FocusTime.App/bin/Debug/net8.0-windows/`
- Release: `src/FocusTime.App/bin/Release/net8.0-windows/`

### Bước 4: Run Application

```powershell
# Run trực tiếp
dotnet run --project src/FocusTime.App

# Hoặc chạy file .exe
cd src/FocusTime.App/bin/Debug/net8.0-windows
./FocusTime.App.exe
```

---

## Publish Executable

### Option 1: Self-Contained Single File (Khuyến nghị)

Tạo file .exe độc lập, không cần cài .NET Runtime:

```powershell
dotnet publish src/FocusTime.App `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:PublishReadyToRun=true `
  -p:EnableCompressionInSingleFile=true
```

**Output:** `src/FocusTime.App/bin/Release/net8.0-windows/win-x64/publish/FocusTime.App.exe`

**Kích thước:** ~80-120 MB (bao gồm .NET runtime)

**Ưu điểm:**
- ✅ Chạy ngay trên Windows 10/11 sạch
- ✅ Không cần cài .NET Runtime
- ✅ Single file dễ deploy

### Option 2: Framework-Dependent

Tạo .exe nhẹ hơn, cần .NET Runtime:

```powershell
dotnet publish src/FocusTime.App `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=true
```

**Output:** `src/FocusTime.App/bin/Release/net8.0-windows/win-x64/publish/FocusTime.App.exe`

**Kích thước:** ~5-10 MB

**Yêu cầu:** Máy target phải có .NET 8 Runtime

### Option 3: Portable Folder

Tạo folder với tất cả DLLs:

```powershell
dotnet publish src/FocusTime.App -c Release -r win-x64
```

**Output:** Folder `publish/` chứa nhiều files

---

## Testing

### Unit Tests (Chưa implement)

Để thêm tests sau này:

```powershell
# Tạo test project
dotnet new xunit -n FocusTime.Tests
dotnet sln add FocusTime.Tests/FocusTime.Tests.csproj

# Run tests
dotnet test
```

### Manual Testing Checklist

Sau khi build, test các tính năng chính:

- [ ] App khởi động thành công
- [ ] Timer đếm ngược đúng
- [ ] Start/Pause/Resume hoạt động
- [ ] Skip Break hoạt động
- [ ] WebView2 load được trang web
- [ ] Domain blocking hoạt động (test youtube.com)
- [ ] Add/Edit task hoạt động
- [ ] History window mở được
- [ ] Settings window mở được
- [ ] Data lưu vào `%APPDATA%\FocusTime\data.json`
- [ ] Notifications (popup) hiển thị

### Performance Testing

```powershell
# Monitor memory usage
Get-Process FocusTime.App | Select-Object Name, CPU, PM

# Expected: < 200 MB RAM trong session thường
```

---

## Common Issues

### Issue: "The command could not be loaded"

**Nguyên nhân:** .NET SDK chưa được cài hoặc chưa trong PATH

**Giải pháp:**
1. Cài .NET 8 SDK
2. Restart terminal
3. Verify: `dotnet --version`

### Issue: "WebView2 runtime not found"

**Nguyên nhân:** WebView2 Runtime chưa có trên máy

**Giải pháp:**
1. Tải: [https://go.microsoft.com/fwlink/p/?LinkId=2124703](https://go.microsoft.com/fwlink/p/?LinkId=2124703)
2. Cài đặt WebView2 Runtime
3. Restart app

### Issue: Build warnings về nullable references

**Không ảnh hưởng:** Warnings về nullable được enable trong project, nhưng không ảnh hưởng runtime.

**Để tắt warnings:**
```xml
<!-- Thêm vào .csproj -->
<Nullable>disable</Nullable>
```

### Issue: "The type or namespace 'Wpf' does not exist"

**Nguyên nhân:** Thiếu `<UseWPF>true</UseWPF>` trong .csproj

**Giải pháp:** Đã có trong project file, verify:
```xml
<PropertyGroup>
  <UseWPF>true</UseWPF>
</PropertyGroup>
```

### Issue: App crash khi start

**Debug:**
```powershell
# Run với debug info
dotnet run --project src/FocusTime.App --verbosity detailed
```

Check log files và exception stack trace.

---

## Build Scripts (Advanced)

### PowerShell Script - Build All

Tạo file `build.ps1`:

```powershell
#!/usr/bin/env pwsh

Write-Host "🔨 Building Focus Time..." -ForegroundColor Cyan

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
dotnet clean

# Restore
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build Debug
Write-Host "Building Debug..." -ForegroundColor Yellow
dotnet build -c Debug

# Build Release
Write-Host "Building Release..." -ForegroundColor Yellow
dotnet build -c Release

# Publish
Write-Host "Publishing self-contained..." -ForegroundColor Yellow
dotnet publish src/FocusTime.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

Write-Host "✅ Build completed!" -ForegroundColor Green
Write-Host "Output: src/FocusTime.App/bin/Release/net8.0-windows/win-x64/publish/FocusTime.App.exe" -ForegroundColor Cyan
```

**Chạy:**
```powershell
./build.ps1
```

---

## Deployment

### Tạo Installer (Advanced)

Dùng tools như:
- **WiX Toolset** - Tạo MSI installer
- **Inno Setup** - Tạo setup wizard
- **Squirrel.Windows** - Auto-updater

### Portable ZIP

```powershell
# Publish
dotnet publish src/FocusTime.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Tạo ZIP
cd src/FocusTime.App/bin/Release/net8.0-windows/win-x64/publish
Compress-Archive -Path FocusTime.App.exe -DestinationPath FocusTime-v1.0.0-win-x64.zip
```

---

## CI/CD (GitHub Actions)

Tạo file `.github/workflows/build.yml`:

```yaml
name: Build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build -c Release --no-restore
    - name: Publish
      run: dotnet publish src/FocusTime.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: FocusTime-win-x64
        path: src/FocusTime.App/bin/Release/net8.0-windows/win-x64/publish/
```

---

## Support

Nếu gặp vấn đề khi build:

1. **Check logs:** Build output thường chỉ rõ lỗi
2. **Clean build:** `dotnet clean` rồi build lại
3. **Update SDK:** Đảm bảo dùng .NET 8.0.x mới nhất
4. **GitHub Issues:** Mở issue tại repo với logs đầy đủ

---

**Happy Building! 🚀**
