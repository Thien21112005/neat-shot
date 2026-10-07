# NeatShot (NeatSnap) 📸

<p align="center">
  <img src="https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/assets/Screenshot/SVG/ic_fluent_screenshot_32_filled.svg" width="96" height="96" alt="NeatShot Logo" />
</p>

<p align="center">
  <strong>Công cụ chụp màn hình gọn nhẹ, hiện đại và thông minh trên Windows.</strong><br>
  <em>Chụp siêu nhanh • Không dính popup rác • Hỗ trợ Ghim ảnh, OCR & Chú thích Vector</em>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white" alt="Windows 10/11" />
  <img src="https://img.shields.io/badge/UI-WPF%20%7C%20WPF--UI-107C41" alt="WPF / WPF-UI" />
  <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License MIT" />
</p>

---

## 📖 Giới thiệu (Overview)

**NeatShot** là ứng dụng desktop độc lập được xây dựng bằng C# và Windows Presentation Foundation (WPF). Ứng dụng giải quyết triệt để vấn đề thường gặp trên các công cụ chụp màn hình truyền thống (như Lightshot trên Windows 11): **bị dính popup khay hệ thống (tray menu) vào ảnh chụp** và **lệch toạ độ trên màn hình có tỉ lệ hiển thị (DPI scale) khác nhau**.

---

## ✨ Tính năng nổi bật (Features)

### 🚀 Giai đoạn 1: Cốt lõi (Core / MVP)
- 🎯 **Chụp sạch (Clean Shot Algorithm):** Tự động gửi tín hiệu đóng mọi popup của khay hệ thống, trễ 120ms chuẩn xác trước khi chụp để đảm bảo bức ảnh hoàn toàn tinh chỉnh.
- ⌨️ **Phím tắt toàn cục:** Gọi nhanh bằng `PrtSc` hoặc phím tuỳ chỉnh kể cả khi ứng dụng đang chạy nền.
- 🖥️ **Màn hình đóng băng (Freeze Overlay):** Giữ nguyên trạng thái màn hình để người dùng kéo chọn vùng.
- 🎨 **Bộ công cụ chú thích Vector (Vector Canvas):**
  - Bút vẽ tự do (Pencil)
  - Khung chữ nhật (Rectangle)
  - Mũi tên chỉ dẫn (Arrow)
  - Bảng chọn màu và kích thước nét vẽ
  - Hoàn tác tức thì (`Undo / Ctrl+Z`)
- 💾 **Xuất dữ liệu linh hoạt:** Lưu nhanh file ảnh PNG hoặc sao chép thẳng vào Clipboard.
- 🔔 **Chạy nền siêu nhẹ:** Tích hợp sâu dưới khay hệ thống (System Tray), mức tiêu thụ CPU và RAM cực thấp khi nhàn rỗi.

### 🌟 Giai đoạn 2: Tính năng đột phá (Killer Features)
- 📌 **Pin to Screen:** Ghim vùng ảnh chụp thành cửa sổ nổi Always-on-Top để đối chiếu code hoặc tài liệu.
- 🔍 **Quick OCR:** Nhận diện và copy chữ offline tức thì thông qua Windows Media OCR API.
- 🌫️ **Smart Blur / Pixelate:** Làm mờ hoặc khảm hạt để che mật khẩu, token, thông tin riêng tư.
- ① **Step Counter:** Đánh số bước tự động (①, ②, ③...) phục vụ viết tài liệu và báo cáo lỗi.
- ✨ **Beautify:** Bo góc, đổ bóng mềm và thêm nền gradient nghệ thuật trước khi chia sẻ.

---

## 🏗️ Kiến trúc & Công nghệ (Architecture & Tech Stack)

Hệ thống được thiết kế theo mô hình **Clean Layered Architecture kết hợp MVVM** để đảm bảo khả năng mở rộng và dễ dàng viết Unit Test:

```text
NeatShot/
├── src/
│   └── NeatShot/                       # Dự án WPF Desktop chính
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
    ├── SKILLS-WORKFLOW-GUIDE.md         # Quy trình kỹ thuật & Agent Skills
    └── superpowers/plans/               # Kế hoạch thực thi chi tiết
```

### Công nghệ sử dụng:
* **Ngôn ngữ:** C# 12 / .NET 10 (Target: `net10.0-windows10.0.19041.0`)
* **Giao diện:** WPF + [`WPF-UI`](https://github.com/lepoco/wpfui) (Fluent Design)
* **MVVM:** [`CommunityToolkit.Mvvm`](https://github.com/CommunityToolkit/dotnet)
* **Khay hệ thống:** [`H.NotifyIcon.Wpf`](https://github.com/HavenDV/H.NotifyIcon)
* **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`
* **Kiểm thử:** xUnit

---

## 🚀 Hướng dẫn cài đặt & Chạy dự án (Getting Started)

### Yêu cầu hệ thống:
* Hệ điều hành: Windows 10 (version 1809 trở lên) hoặc Windows 11.
* SDK: [.NET 10 SDK](https://dotnet.microsoft.com/download) (hoặc Visual Studio 2022 / JetBrains Rider).

### 1. Clone repository:
```bash
git clone https://github.com/Thien21112005/neat-shot.git
cd neat-shot
```

### 2. Khôi phục packages & Build dự án:
```bash
dotnet restore
dotnet build
```

### 3. Chạy toàn bộ Unit Tests:
```bash
dotnet test
```

### 4. Khởi chạy ứng dụng:
```bash
dotnet run --project src/NeatShot
```

---

## 📚 Tài liệu chi tiết (Documentation)

Bạn có thể tham khảo các tài liệu chuyên sâu trong thư mục [`docs/`](./docs/):
- 📄 [SRS - Đặc tả yêu cầu chức năng & phi chức năng](./docs/SRS-screenshot-app.md)
- 🏛️ [Kiến trúc hệ thống & Thiết kế kỹ thuật](./docs/TECH-STACK-AND-ARCHITECTURE.md)
- 🧭 [Sổ tay quy trình phát triển & Agent Skills](./docs/SKILLS-WORKFLOW-GUIDE.md)
- 📝 [Kế hoạch triển khai Phase 1 MVP](./docs/superpowers/plans/2026-10-07-neatshot-phase1-mvp.md)

---

## 🤝 Quy chuẩn Commit (Conventional Commits)

Dự án áp dụng quy chuẩn commit quốc tế:
- `feat:` Tính năng mới (ví dụ: `feat: implement clean screen capture service`)
- `fix:` Sửa lỗi (ví dụ: `fix: resolve coordinate offset on 125% DPI`)
- `test:` Bổ sung hoặc cập nhật unit tests (ví dụ: `test: add tests for DpiHelper`)
- `docs:` Thay đổi tài liệu (ví dụ: `docs: update architecture overview`)
- `chore:` Công việc bảo trì, cấu hình build (ví dụ: `chore: update dependencies`)

---

## 📄 Bản quyền (License)

Dự án được phân phối dưới giấy phép **MIT License**. Chi tiết xem tại [LICENSE](./LICENSE).
