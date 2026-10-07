<p align="center">
  <img src="https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/assets/Screenshot/SVG/ic_fluent_screenshot_32_filled.svg" width="96" height="96" alt="NeatShot Logo" />
</p>

<h1 align="center">NeatShot (NeatSnap)</h1>

<p align="center">
  A modern, lightweight, and clean screenshot utility for Windows.<br>
  Instant capture &bull; Clean desktop capture &bull; Pin to screen, offline OCR &bull; Vector annotations
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?style=flat&logo=windows&logoColor=white" alt="Windows 10/11" />
  <img src="https://img.shields.io/badge/UI-WPF%20%7C%20WPF--UI-107C41?style=flat" alt="WPF / WPF-UI" />
  <img src="https://img.shields.io/badge/Architecture-MVVM-informational?style=flat" alt="MVVM" />
  <img src="https://img.shields.io/badge/License-MIT-blue.svg?style=flat" alt="License MIT" />
</p>

<p align="center">
  <a href="#english">English</a> &bull; <a href="#tiếng-việt">Tiếng Việt</a>
</p>

---

<div id="english"></div>

## English

### Overview

**NeatShot** is an independent Windows desktop application built with C# and Windows Presentation Foundation (WPF). It addresses longstanding issues found in traditional screenshot tools on Windows 11:
- Popup menus from the system tray getting captured in screenshots.
- Global shortcut collisions.
- Coordinate and rendering discrepancies caused by multi-monitor setups with mixed display scaling (100%, 125%, 150% Per-Monitor DPI).

---

### Features

#### Phase 1: Core / MVP
- **Clean Shot Algorithm:** Automatically dismisses active system popups and applies a precise 120ms delay before capturing to ensure pristine images.
- **Global Hotkey:** Trigger capture instantly via configurable shortcuts (e.g., `PrtSc`) even when the app is minimized to the system tray.
- **Freeze Overlay:** Freezes the screen state into a full-screen transparent canvas across all virtual displays.
- **Vector Annotation Tools:**
  - Freehand pencil
  - Rectangle highlighter
  - Directional arrows and lines
  - Customizable color palette and stroke width
  - Instant undo stack (`Ctrl + Z`)
- **Flexible Export:** Instant copy to clipboard and save to PNG format.
- **Lightweight Background Operation:** Runs quietly in the system tray with near-zero idle CPU and memory consumption.

#### Phase 2: Killer Features
- **Pin to Screen:** Float captured regions in an Always-on-Top borderless window for side-by-side reference.
- **Quick OCR:** Extract text from image regions directly into the clipboard using Windows Media OCR offline.
- **Smart Blur / Pixelate:** Conceal sensitive information such as passwords, tokens, and credentials.
- **Step Counter:** Auto-incrementing step indicators (1, 2, 3...) for generating technical guides and bug reports.
- **Beautify:** Add customizable padding, rounded corners, soft shadows, and gradient backgrounds.

---

### Architecture & Tech Stack

NeatShot adheres to **Clean Layered Architecture combined with the MVVM pattern**:

```text
NeatShot/
├── src/
│   └── NeatShot/                       # Main WPF Desktop Application
│       ├── App.xaml / App.xaml.cs       # Application Entry Point & DI Container
│       ├── app.manifest                 # PerMonitorV2 DPI Configuration
│       ├── Common/                      # Helpers (DpiHelper, ColorHelper)
│       ├── Core/                        # Business Logic & Native Interop
│       │   ├── Interop/                 # NativeMethods, NativeStructs (P/Invoke)
│       │   ├── Models/                  # CaptureRegion, DrawingElement
│       │   └── Services/                # ScreenCaptureService, HotkeyService, ExportService
│       ├── Presentation/                # UI Layer (WPF / MVVM)
│       │   ├── ViewModels/              # OverlayViewModel, PinViewModel
│       │   ├── Views/                   # OverlayWindow.xaml, PinWindow.xaml
│       │   └── Controls/                # SelectionCanvas, AnnotationToolbar
│       └── Assets/                      # Icons, Styles
│
├── tests/
│   └── NeatShot.Tests/                  # Independent Unit Test Suite (xUnit)
│
└── docs/                                # Technical Documentation
    ├── SRS-screenshot-app.md            # Software Requirements Specification
    ├── TECH-STACK-AND-ARCHITECTURE.md   # Architectural & Tech Design Document
    ├── SKILLS-WORKFLOW-GUIDE.md         # Engineering Workflow & Skills Playbook
    └── superpowers/plans/               # Detailed Implementation Plans
```

#### Technologies:
- **Language & Runtime:** C# 12 / .NET 10 (`net10.0-windows10.0.19041.0`)
- **UI Framework:** WPF + [WPF-UI](https://github.com/lepoco/wpfui) (Fluent Design)
- **MVVM Framework:** [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- **Tray Icon:** [H.NotifyIcon.Wpf](https://github.com/HavenDV/H.NotifyIcon)
- **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`
- **Testing:** xUnit

---

### Getting Started

#### Prerequisites
- Windows 10 (version 1809 or higher) or Windows 11.
- [.NET 10 SDK](https://dotnet.microsoft.com/download) or Visual Studio 2022 / JetBrains Rider.

#### Build & Run
```bash
# Clone the repository
git clone https://github.com/Thien21112005/neat-shot.git
cd neat-shot

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run unit tests
dotnet test

# Launch application
dotnet run --project src/NeatShot
```

---

### Documentation

Technical documents are located in the [`docs/`](./docs/) directory:
- [Software Requirements Specification (SRS)](./docs/SRS-screenshot-app.md)
- [System Architecture & Tech Stack](./docs/TECH-STACK-AND-ARCHITECTURE.md)
- [Skills & Workflow Guide](./docs/SKILLS-WORKFLOW-GUIDE.md)
- [Phase 1 MVP Implementation Plan](./docs/superpowers/plans/2026-10-07-neatshot-phase1-mvp.md)

---

### Commit Convention

This project enforces Conventional Commits:
- `feat:` New features
- `fix:` Bug fixes
- `test:` Adding or updating tests
- `docs:` Documentation changes
- `chore:` Build configuration and maintenance

---

### License

Distributed under the **MIT License**. See [LICENSE](./LICENSE) for details.

---

<div id="tiếng-việt"></div>

## Tiếng Việt

### Giới thiệu

**NeatShot** là ứng dụng chụp màn hình độc lập dành cho Windows, được phát triển bằng C# và Windows Presentation Foundation (WPF). Dự án giải quyết các vấn đề cố hữu trên các công cụ chụp màn hình truyền thống khi hoạt động trên Windows 11:
- Bị dính menu/popup của khay hệ thống vào ảnh chụp.
- Phím tắt toàn cục hay bị xung đột hoặc bị chiếm dụng.
- Lệch toạ độ và sai lệch độ phân giải khi sử dụng đa màn hình có tỉ lệ thu phóng khác nhau (100%, 125%, 150% Per-Monitor DPI).

---

### Tính năng chính

#### Giai đoạn 1: Cốt lõi (Core / MVP)
- **Thuật toán Chụp sạch (Clean Shot):** Tự động gửi tín hiệu ẩn toàn bộ popup và đợi 120ms trước khi bắt đầu chụp, đảm bảo bức ảnh không dính giao diện thừa.
- **Phím tắt toàn cục:** Gọi nhanh công cụ bằng phím tắt do người dùng cấu hình (mặc định `PrtSc`) kể cả khi ứng dụng đang chạy ẩn.
- **Màn hình đóng băng (Freeze Overlay):** Giữ nguyên trạng thái toàn bộ màn hình để người dùng kéo thả chọn vùng cần lấy.
- **Bộ công cụ chú thích Vector:**
  - Bút vẽ tự do (Pencil)
  - Khung chữ nhật (Rectangle)
  - Mũi tên chỉ dẫn (Arrow)
  - Bảng chọn màu sắc và độ dày nét vẽ
  - Hoàn tác tức thì (`Undo / Ctrl + Z`)
- **Xuất ảnh linh hoạt:** Lưu nhanh file PNG hoặc sao chép thẳng vào Clipboard.
- **Chạy nền tối ưu:** Ẩn mình dưới khay hệ thống (System Tray), mức chiếm dụng CPU và RAM khi nhàn rỗi gần như bằng không.

#### Giai đoạn 2: Tính năng đột phá
- **Ghim ảnh nổi (Pin to Screen):** Đưa vùng ảnh chụp thành cửa sổ nổi Always-on-Top để đối chiếu tài liệu và mã nguồn.
- **Trích xuất chữ (Quick OCR):** Nhận diện văn bản offline và copy text vào Clipboard thông qua Windows Media OCR API.
- **Bảo mật (Smart Blur / Pixelate):** Bôi mờ hoặc khảm hạt để che thông tin nhạy cảm như token, mật khẩu, thông tin cá nhân.
- **Đánh số bước (Step Counter):** Đánh số tăng dần tự động (1, 2, 3...) phục vụ viết tài liệu hướng dẫn và báo cáo lỗi.
- **Làm đẹp ảnh (Beautify):** Tự động bo tròn góc, đổ bóng mềm và thêm nền gradient nghệ thuật trước khi lưu.

---

### Kiến trúc & Công nghệ

Hệ thống được thiết kế theo mô hình **Clean Layered Architecture kết hợp MVVM**:

```text
NeatShot/
├── src/
│   └── NeatShot/                       # Dự án ứng dụng WPF Desktop chính
│       ├── App.xaml / App.xaml.cs       # Khởi động ứng dụng & Cấu hình DI
│       ├── app.manifest                 # Bật Per-Monitor DPI v2
│       ├── Common/                      # Helpers (DpiHelper, ColorHelper)
│       ├── Core/                        # Tầng Nghiệp vụ & Interop Win32
│       │   ├── Interop/                 # NativeMethods, NativeStructs (P/Invoke)
│       │   ├── Models/                  # CaptureRegion, DrawingElement
│       │   └── Services/                # ScreenCaptureService, HotkeyService, ExportService
│       ├── Presentation/                # Tầng Giao diện (WPF / MVVM)
│       │   ├── ViewModels/              # OverlayViewModel, PinViewModel
│       │   ├── Views/                   # OverlayWindow.xaml, PinWindow.xaml
│       │   └── Controls/                # SelectionCanvas, AnnotationToolbar
│       └── Assets/                      # Icons, Styles
│
├── tests/
│   └── NeatShot.Tests/                  # Dự án Unit Test độc lập (xUnit)
│
└── docs/                                # Tài liệu kỹ thuật dự án
    ├── SRS-screenshot-app.md            # Đặc tả yêu cầu phần mềm
    ├── TECH-STACK-AND-ARCHITECTURE.md   # Thiết kế kiến trúc & công nghệ
    ├── SKILLS-WORKFLOW-GUIDE.md         # Sổ tay quy trình kỹ thuật & Skills
    └── superpowers/plans/               # Kế hoạch triển khai chi tiết
```

#### Công nghệ sử dụng:
- **Ngôn ngữ & Nền tảng:** C# 12 / .NET 10 (`net10.0-windows10.0.19041.0`)
- **Giao diện:** WPF + [WPF-UI](https://github.com/lepoco/wpfui) (Fluent Design)
- **Mô hình MVVM:** [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- **Khay hệ thống:** [H.NotifyIcon.Wpf](https://github.com/HavenDV/H.NotifyIcon)
- **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`
- **Kiểm thử tự động:** xUnit

---

### Hướng dẫn chạy dự án

#### Yêu cầu môi trường
- Windows 10 (bản 1809 trở lên) hoặc Windows 11.
- [.NET 10 SDK](https://dotnet.microsoft.com/download) hoặc Visual Studio 2022 / JetBrains Rider.

#### Các bước thực hiện
```bash
# Clone mã nguồn
git clone https://github.com/Thien21112005/neat-shot.git
cd neat-shot

# Khôi phục dependencies
dotnet restore

# Build mã nguồn
dotnet build

# Chạy kiểm thử tự động
dotnet test

# Chạy ứng dụng
dotnet run --project src/NeatShot
```

---

### Tài liệu tham khảo

Các tài liệu kỹ thuật chi tiết được lưu trữ trong thư mục [`docs/`](./docs/):
- [Đặc tả yêu cầu phần mềm (SRS)](./docs/SRS-screenshot-app.md)
- [Thiết kế kiến trúc & Công nghệ](./docs/TECH-STACK-AND-ARCHITECTURE.md)
- [Sổ tay quy trình & Agent Skills](./docs/SKILLS-WORKFLOW-GUIDE.md)
- [Kế hoạch triển khai Phase 1 MVP](./docs/superpowers/plans/2026-10-07-neatshot-phase1-mvp.md)

---

### Quy ước Commit

Dự án áp dụng chuẩn Conventional Commits:
- `feat:` Bổ sung tính năng mới
- `fix:` Sửa lỗi
- `test:` Bổ sung hoặc cập nhật bài kiểm thử
- `docs:` Cập nhật tài liệu
- `chore:` Công việc cấu hình hoặc bảo trì dự án

---

### Giấy phép

Mã nguồn được phân phối theo giấy phép **MIT License**. Chi tiết xem tại [LICENSE](./LICENSE).
