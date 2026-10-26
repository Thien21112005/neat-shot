<h1 align="center">NeatShot</h1>

<p align="center">
  A fast, lightweight screenshot tool for Windows, built for multi-monitor and mixed-DPI setups.<br>
  Clean capture &bull; Vector annotations &bull; Pin to screen &bull; Offline text extraction &bull; Screenshot gallery
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20(2004%2B)%20%7C%2011-0078D6?style=flat&logo=windows&logoColor=white" alt="Windows 10 2004+ / 11" />
  <img src="https://img.shields.io/badge/UI-WPF%20%2B%20WPF--UI-107C41?style=flat" alt="WPF + WPF-UI" />
  <img src="https://img.shields.io/badge/Tests-xUnit-5E5E5E?style=flat" alt="xUnit" />
  <img src="https://img.shields.io/badge/License-MIT-blue.svg?style=flat" alt="MIT License" />
</p>

<p align="center">
  <a href="#english">English</a> &bull; <a href="#tiếng-việt">Tiếng Việt</a>
</p>

---

<div id="english"></div>

## English

### Table of Contents

- [Why NeatShot](#why-neatshot)
- [Features](#features)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Getting Started](#getting-started)
- [Running Tests](#running-tests)
- [Where Files Are Stored](#where-files-are-stored)
- [Architecture](#architecture)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

### Why NeatShot

Built-in and classic screenshot tools on Windows 11 often:

- capture the tray flyout or context menu that was used to start the capture;
- lose their global shortcut to another app (for example, Snipping Tool taking over `PrtSc`);
- select the wrong area on multi-monitor setups with mixed scaling (100% / 125% / 150%).

NeatShot runs in the system tray. It freezes the whole virtual desktop into one overlay and lets you select, annotate and export a region in a few keystrokes.

### Features

**Capture**
- **Clean Shot:** dismisses tray flyouts and popups, then waits 250 ms for Windows animations to finish before capturing with GDI `BitBlt` (`CAPTUREBLT` included for layered windows).
- **Freeze overlay** that spans every monitor, with a resizable and movable selection (8 handles plus a live `W × H` badge).
- **Window detection:** hover to outline the window (or the panel/button) under the cursor, then click once to select exactly that area — no more dragging edge to edge.
- **Full-screen capture** from the tray menu, with the whole virtual screen already selected.
- **Repeat the last region** (`Ctrl + Shift + R`): reopens the capture screen with the region you used last time already selected — handy for before/after comparisons. If a monitor was unplugged, the region is trimmed to the current screen.
- **Delayed capture** (tray → *Chụp hẹn giờ*: 3, 5 or 10 s): open the menu or tooltip you need, and NeatShot captures it without closing it. Press `Esc` to cancel the countdown.
- Per-Monitor V2 DPI awareness, so selections map exactly to physical pixels.

**Annotate**
- Pencil, highlighter, rectangle, ellipse, line, arrow and text (font family and size can be changed).
- **Step counter** badges (1, 2, 3…) with adjustable size.
- **Pixelate** and **blur** tools to hide sensitive data.
- Select and move, delete, and unlimited undo/redo.
- Color palette, screen **eyedropper** with a magnifier, and a recent-colors history that is saved between sessions.
- **Beautify:** gradient background, padding, rounded corners and a soft shadow, shown in a live preview.

**Export & reuse**
- Copy to clipboard, save as PNG or JPG, or **pin** the capture as an always-on-top floating window.
- Copied and pinned captures are **saved automatically** to `Pictures\NeatShot`.
- **Pin from the clipboard** (`F3`): pin any image you copied from a browser, chat app, Figma or Explorer — no capture needed.
- **Pinned windows:** zoom with the mouse wheel, change opacity with Ctrl + wheel, draw annotations directly on the pin, copy the annotated result, and keep as many pins open as you need.
- **Text extraction (OCR):** fully offline through `Windows.Media.Ocr`. It keeps line breaks and normalizes Vietnamese diacritics.
- **Screenshot gallery** (tray menu): browse thumbnails, then pin, copy or delete any past capture.

**Everyday**
- **Settings window** (tray → *Cài đặt…*): change hotkeys on the fly (with a warning if another app already uses a key), pick the save folder, PNG or JPG (with quality), auto-save to the gallery, start with Windows, "copy as soon as I release the mouse" for one-step captures, and a light, dark or system theme.

### Keyboard Shortcuts

| Context | Shortcut | Action |
|---|---|---|
| Global | `Ctrl + Shift + A` / `PrtSc` | Start a capture (both keys can be changed in Settings) |
| Global | `F3` | Pin the image in the clipboard |
| Global | `Ctrl + Shift + R` | Capture the last region again |
| Overlay | Click (no drag) | Select the highlighted window |
| Overlay | `Enter` / `Ctrl + C` | Copy to clipboard (also saved to the gallery) |
| Overlay | `Ctrl + S` | Save to the screenshots folder |
| Overlay | `Ctrl + Shift + S` (or Shift + click Save) | Save as… (choose folder and name) |
| Overlay | `Ctrl + O` | Extract text (OCR) |
| Overlay | `Ctrl + Z` / `Ctrl + Y` (`Ctrl + Shift + Z`) | Undo / Redo |
| Overlay | `V` `P` `R` `O` `L` `A` `H` `T` `B` `N` `I` | Select, Pencil, Rectangle, Ellipse, Line, Arrow, Highlight, Text, Pixelate, Step, Eyedropper |
| Overlay | `Delete` | Delete the selected annotation |
| Overlay | `Esc` / right-click | Leave the current tool; press again to cancel the capture |
| Pin | Mouse wheel / `+` `-` / `Ctrl + 0` | Zoom in, zoom out, reset |
| Pin | `Ctrl + wheel` | Change opacity |
| Pin | `P` / `Ctrl + P` | Show or hide the annotation toolbar |
| Pin | `Ctrl + C` | Copy the pinned image, including annotations |
| Pin | `Esc` | Hide the controls; press again to close the pin |
| Gallery | `F5` / `Esc` | Refresh / Close |

### Getting Started

**Download (recommended)**
1. Download `NeatShot-v<version>-win-x64.zip` from the [latest release](https://github.com/Thien21112005/neat-shot/releases/latest).
2. Extract it and run `NeatShot.exe`. No installer and no .NET runtime needed (self-contained).
3. If Windows SmartScreen warns about an unknown publisher (the build is not code-signed yet), choose **More info → Run anyway**.

**Requirements**
- Windows 10 version 2004 (build 19041) or later, or Windows 11 (x64).
- An OCR language pack (installed with most Windows display languages) to use text extraction.

**Build from source** (requires the [.NET 10 SDK](https://dotnet.microsoft.com/download))
```bash
git clone https://github.com/Thien21112005/neat-shot.git
cd neat-shot

dotnet build
dotnet run --project src/NeatShot
```

NeatShot starts in the system tray. Left-click the tray icon to capture, or right-click it to open the menu (full-screen capture, gallery, screenshots folder, start with Windows, exit). Only one instance can run at a time.

**Start with Windows:** like Lightshot, NeatShot registers itself to start in the tray when you sign in, so `Ctrl + Shift + A` is always ready. Turn it off with the tray menu item *Khởi động cùng Windows* (or in *Task Manager → Startup apps*). If you move the NeatShot folder, just run it once from the new location.

**Package a release build:** `./scripts/publish.ps1` runs the tests and produces `artifacts/NeatShot-v<version>-win-x64.zip`. See [CHANGELOG.md](./CHANGELOG.md) for release notes.

**Microsoft Store package (MSIX):** `./scripts/package-msix.ps1` runs the tests and produces `artifacts/msix/NeatShot-<version>.0-x64.msix` from the manifest and logos in `packaging/msix/`. The package is unsigned on purpose: the Store signs it on release. Add `-Register` to install the build locally first (requires Windows *Developer Mode*). In the Store version, start-with-Windows is a Windows startup task you can also manage in *Settings → Apps → Startup*. Store listing copy lives in [docs/store/store-listing.md](./docs/store/store-listing.md) and the privacy policy in [site/privacy.html](./site/privacy.html).

### Running Tests

```bash
# Full suite
dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj

# Skip the tests that capture the real desktop
dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj --filter "Category!=Desktop"
```

> Tests tagged `Category=Desktop` run the real Clean Shot routine, which sends a global `Esc` keystroke. Exclude them when running tests from an IDE or agent that `Esc` could interrupt.

### Where Files Are Stored

| Data | Location |
|---|---|
| Screenshots (gallery) | `%USERPROFILE%\Pictures\NeatShot` (or the folder chosen in Settings / *Save as*); `%USERPROFILE%\NeatShot` if Windows blocks writing there |
| Settings & color history | `%AppData%\NeatShot\settings.json` (Store version: kept by Windows in the app's private storage) |

### Architecture

NeatShot uses a layered design (Core / Presentation) with MVVM via CommunityToolkit.Mvvm and constructor injection via `Microsoft.Extensions.DependencyInjection`.

```text
src/NeatShot/
├── App.xaml(.cs)          # Entry point, DI container, tray menu, single-instance mutex
├── app.manifest           # Per-Monitor V2 DPI awareness
├── Common/Helpers/        # DPI, color, cursor, hit-testing, image effects, OCR post-processing
├── Core/
│   ├── Interop/           # Win32 P/Invoke (GDI capture, hotkeys, Shell_NotifyIcon)
│   ├── Models/            # CaptureRegion, DrawingElement, BeautifyOptions, GalleryItem, AppSettings
│   └── Services/          # Capture, Hotkey, Export, Beautify, OCR, Settings, Gallery, UndoStack
└── Presentation/
    ├── Controls/          # SelectionCanvas, DrawingCanvas, AnnotationToolbar, PinAnnotationToolbar
    ├── Tray/              # NativeTrayIcon (pure Win32, no third-party dependency)
    ├── ViewModels/        # Overlay, Pin, Gallery
    └── Views/             # OverlayWindow, PinWindow, GalleryWindow
tests/NeatShot.Tests/      # xUnit suite (WPF tests run on dedicated STA threads)
site/                      # Landing page (static HTML/CSS/JS + GSAP) — see site/README.md
docs/                      # SRS, architecture, workflow guide, plans, design exports
```

**Tech stack:** C# / .NET 10 (`net10.0-windows10.0.19041.0`), WPF + [WPF-UI](https://github.com/lepoco/wpfui), [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet), `Microsoft.Extensions.DependencyInjection`, `Windows.Media.Ocr`, xUnit.

More detail: [SRS](./docs/SRS-screenshot-app.md) · [Architecture & tech stack](./docs/TECH-STACK-AND-ARCHITECTURE.md) · [Workflow guide](./docs/SKILLS-WORKFLOW-GUIDE.md) · [Implementation plans](./docs/superpowers/plans/)

### Troubleshooting

- **`PrtSc` does nothing.** Windows 11 may give `PrtSc` to Snipping Tool. Turn off *Settings → Accessibility → Keyboard → "Use the Print screen key to open screen capture"*, or use `Ctrl + Shift + A`. The console output at startup shows which hotkeys were registered.
- **Screenshots end up in `%USERPROFILE%\NeatShot` instead of Pictures.** *Controlled folder access* (Windows Security → Virus & threat protection → Ransomware protection) stops unknown apps from writing to Pictures, Documents or Desktop. NeatShot then saves to `%USERPROFILE%\NeatShot` instead (the "saved" toast shows the folder). To use Pictures again, choose *Allow an app through Controlled folder access* and add `NeatShot.exe`, or pick another folder in Settings.
- **"No OCR language pack found."** Add a language that supports OCR under *Settings → Time & language → Language & region*.
- **The tray icon disappeared.** NeatShot re-registers its icon when Explorer restarts. If it is still missing, the app may have exited; start it again.

### Contributing

1. Create a branch from `main` (`feat/...`, `fix/...`, `docs/...`).
2. Follow TDD: write a failing test, make it pass, then refactor.
3. Make sure `dotnet test` passes before you open a PR.
4. Use [Conventional Commits](https://www.conventionalcommits.org/), for example `feat(pin): ...`, `fix(gallery): ...`, `docs(readme): ...`, `test(...)`, `chore(...)`.

### License

Distributed under the **MIT License**. See [LICENSE](./LICENSE).

---

<div id="tiếng-việt"></div>

## Tiếng Việt

### Giới thiệu

**NeatShot** là ứng dụng chụp màn hình gọn nhẹ cho Windows, viết bằng C# / WPF. Ứng dụng giải quyết các vấn đề quen thuộc trên Windows 11:

- Ảnh chụp bị dính menu hoặc flyout của khay hệ thống.
- Phím tắt toàn cục bị ứng dụng khác chiếm (ví dụ Snipping Tool chiếm `PrtSc`).
- Lệch vùng chọn khi dùng nhiều màn hình có tỉ lệ thu phóng khác nhau (100% / 125% / 150%).

### Tính năng chính

**Chụp ảnh**
- **Chụp sạch (Clean Shot):** đóng flyout/popup, chờ 250 ms cho animation của Windows kết thúc rồi mới chụp bằng GDI `BitBlt`.
- **Màn hình đóng băng** trải trên mọi màn hình. Vùng chọn kéo giãn và di chuyển được (8 tay nắm, hiển thị kích thước `W × H`).
- **Tự nhận diện cửa sổ:** rê chuột để viền sẵn cửa sổ (hoặc panel/nút) dưới con trỏ, click một lần là chọn đúng vùng đó — không cần kéo từng mép.
- **Chụp toàn màn hình** từ menu khay, toàn bộ màn hình được chọn sẵn.
- **Chụp lại vùng vừa chụp** (`Ctrl + Shift + R`): mở màn hình chụp với đúng vùng lần trước được chọn sẵn — tiện so sánh trước/sau. Nếu đã rút màn hình phụ, vùng được tự cắt gọn trong màn hình hiện tại.
- **Chụp hẹn giờ** (menu khay → *Chụp hẹn giờ*: 3, 5 hoặc 10 giây): mở sẵn menu hay tooltip cần chụp, NeatShot chụp mà không làm chúng biến mất. Nhấn `Esc` để huỷ đếm ngược.
- Hỗ trợ Per-Monitor DPI V2, toạ độ khớp chính xác với pixel vật lý.

**Chú thích**
- Bút vẽ, bút dạ quang, hình chữ nhật, oval, đường thẳng, mũi tên, chữ (chọn được phông và cỡ chữ).
- **Đánh số bước** (1, 2, 3…), chỉnh được kích thước.
- **Khảm ô vuông** và **làm mờ** để che thông tin nhạy cảm.
- Chọn/di chuyển, xoá nét vẽ, hoàn tác và làm lại không giới hạn.
- Bảng màu, **bút hút màu** có kính lúp, lịch sử màu gần đây được lưu lại giữa các lần dùng.
- **Làm đẹp ảnh:** nền gradient, khoảng đệm, bo góc, đổ bóng mềm, có xem trước trực tiếp.

**Xuất và tái sử dụng**
- Sao chép vào Clipboard, lưu PNG hoặc JPG, hoặc **ghim** ảnh thành cửa sổ nổi luôn trên cùng.
- Ảnh đã sao chép hoặc ghim được **tự động lưu** vào `Pictures\NeatShot`.
- **Ghim ảnh từ Clipboard** (`F3`): ghim bất kỳ ảnh nào bạn vừa copy từ trình duyệt, Zalo, Figma hay Explorer — không cần chụp.
- **Ảnh ghim:** lăn chuột để phóng to/thu nhỏ, Ctrl + lăn để chỉnh độ trong suốt, vẽ chú thích ngay trên ảnh ghim, sao chép cả ảnh lẫn chú thích. Ghim được nhiều ảnh cùng lúc.
- **Trích xuất chữ (OCR)** hoàn toàn offline qua `Windows.Media.Ocr`, giữ nguyên xuống dòng và chuẩn hoá dấu tiếng Việt.
- **Thư viện ảnh chụp** (mở từ menu khay): xem thumbnail, ghim lại, sao chép hoặc xoá ảnh cũ.

**Dùng hằng ngày**
- **Cửa sổ Cài đặt** (menu khay → *Cài đặt…*): đổi phím tắt ngay lập tức (cảnh báo nếu phím đã bị ứng dụng khác dùng), chọn thư mục lưu, định dạng PNG hoặc JPG (kèm chất lượng), tự lưu vào thư viện, khởi động cùng Windows, "sao chép ngay khi thả chuột" để chụp chỉ với một thao tác, và giao diện sáng, tối hoặc theo hệ thống.

### Phím tắt

| Ngữ cảnh | Phím tắt | Chức năng |
|---|---|---|
| Toàn cục | `Ctrl + Shift + A` / `PrtSc` | Bắt đầu chụp (đổi được cả hai phím trong Cài đặt) |
| Toàn cục | `F3` | Ghim ảnh đang có trong Clipboard |
| Toàn cục | `Ctrl + Shift + R` | Chụp lại vùng vừa chụp |
| Màn hình chụp | Click (không kéo) | Chọn cửa sổ đang được viền |
| Màn hình chụp | `Enter` / `Ctrl + C` | Sao chép (đồng thời lưu vào thư viện) |
| Màn hình chụp | `Ctrl + S` | Lưu thẳng vào thư mục ảnh |
| Màn hình chụp | `Ctrl + Shift + S` (hoặc Shift + click nút Lưu) | Lưu thành… (chọn thư mục và tên file) |
| Màn hình chụp | `Ctrl + O` | Trích xuất chữ |
| Màn hình chụp | `Ctrl + Z` / `Ctrl + Y` (`Ctrl + Shift + Z`) | Hoàn tác / Làm lại |
| Màn hình chụp | `V` `P` `R` `O` `L` `A` `H` `T` `B` `N` `I` | Chọn, Bút, Chữ nhật, Oval, Đường thẳng, Mũi tên, Dạ quang, Chữ, Khảm, Đánh số, Hút màu |
| Màn hình chụp | `Delete` | Xoá nét vẽ đang chọn |
| Màn hình chụp | `Esc` / chuột phải | Thoát công cụ hiện tại; bấm lần nữa để huỷ chụp |
| Ảnh ghim | Lăn chuột / `+` `-` / `Ctrl + 0` | Phóng to, thu nhỏ, về 100% |
| Ảnh ghim | `Ctrl + lăn chuột` | Chỉnh độ trong suốt |
| Ảnh ghim | `P` / `Ctrl + P` | Bật/tắt thanh công cụ vẽ |
| Ảnh ghim | `Ctrl + C` | Sao chép ảnh ghim (kèm chú thích) |
| Ảnh ghim | `Esc` | Ẩn thanh điều khiển; bấm lần nữa để đóng ảnh ghim |
| Thư viện | `F5` / `Esc` | Làm mới / Đóng |

### Cài đặt và chạy

**Tải về (khuyên dùng)**
1. Tải file `NeatShot-v<version>-win-x64.zip` ở [bản phát hành mới nhất](https://github.com/Thien21112005/neat-shot/releases/latest).
2. Giải nén và chạy `NeatShot.exe`. Không cần cài đặt, không cần cài .NET (bản self-contained).
3. Nếu Windows SmartScreen cảnh báo "nhà phát hành không xác định" (bản build chưa ký số), chọn **More info → Run anyway**.

**Yêu cầu**
- Windows 10 bản 2004 (build 19041) trở lên, hoặc Windows 11 (x64).
- Gói ngôn ngữ có hỗ trợ OCR (đa số ngôn ngữ hiển thị của Windows đã có sẵn) để dùng tính năng trích xuất chữ.

**Build từ mã nguồn** (cần [.NET 10 SDK](https://dotnet.microsoft.com/download))

```bash
git clone https://github.com/Thien21112005/neat-shot.git
cd neat-shot

dotnet build
dotnet run --project src/NeatShot
```

Ứng dụng chạy ngầm dưới khay hệ thống. Click trái vào icon để chụp; click phải để mở menu (chụp toàn màn hình, thư viện ảnh, thư mục ảnh, khởi động cùng Windows, thoát). Mỗi lúc chỉ chạy được một phiên bản.

**Khởi động cùng Windows:** giống Lightshot, NeatShot tự đăng ký chạy nền mỗi khi bạn đăng nhập Windows nên `Ctrl + Shift + A` luôn sẵn sàng. Muốn tắt, bỏ chọn mục *Khởi động cùng Windows* trong menu khay (hoặc trong *Task Manager → Startup apps*). Nếu di chuyển thư mục NeatShot, chỉ cần mở app một lần ở vị trí mới.

**Đóng gói bản phát hành:** `./scripts/publish.ps1` sẽ chạy test rồi tạo `artifacts/NeatShot-v<version>-win-x64.zip`. Ghi chú các phiên bản xem tại [CHANGELOG.md](./CHANGELOG.md).

**Gói Microsoft Store (MSIX):** `./scripts/package-msix.ps1` chạy test rồi tạo `artifacts/msix/NeatShot-<version>.0-x64.msix` từ manifest và logo trong `packaging/msix/`. Gói cố ý không ký vì Store sẽ tự ký khi phát hành. Thêm `-Register` để cài thử lên máy trước (cần bật *Developer Mode* của Windows). Ở bản Store, "khởi động cùng Windows" là startup task của Windows, quản lý được cả trong *Settings → Apps → Startup*. Nội dung trang Store nằm ở [docs/store/store-listing.md](./docs/store/store-listing.md), chính sách quyền riêng tư ở [site/privacy.html](./site/privacy.html).

### Chạy kiểm thử

```bash
# Toàn bộ bộ test
dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj

# Bỏ qua các test chụp màn hình thật
dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj --filter "Category!=Desktop"
```

> Các test gắn `Category=Desktop` chạy thuật toán Clean Shot thật nên sẽ gửi phím `Esc` toàn hệ thống. Hãy loại chúng ra khi chạy test trong IDE hoặc agent có thể bị phím `Esc` làm gián đoạn.

### Nơi lưu dữ liệu

| Dữ liệu | Vị trí |
|---|---|
| Ảnh chụp (thư viện) | `%USERPROFILE%\Pictures\NeatShot` (hoặc thư mục chọn trong Cài đặt / *Lưu thành…*); `%USERPROFILE%\NeatShot` nếu Windows chặn ghi vào đó |
| Cấu hình & lịch sử màu | `%AppData%\NeatShot\settings.json` (bản Store: Windows lưu trong vùng riêng của app) |

### Kiến trúc

Dự án chia lớp Core / Presentation, dùng MVVM với CommunityToolkit.Mvvm và tiêm phụ thuộc bằng `Microsoft.Extensions.DependencyInjection`. Xem sơ đồ thư mục ở [phần tiếng Anh](#architecture) và tài liệu chi tiết trong [`docs/`](./docs/).

### Xử lý sự cố

- **Ảnh được lưu vào `%USERPROFILE%\NeatShot` thay vì Pictures:** tính năng *Controlled folder access* (Windows Security → Virus & threat protection → Ransomware protection) chặn app lạ ghi vào Pictures, Documents, Desktop. Khi đó NeatShot lưu vào `%USERPROFILE%\NeatShot` (thông báo "Đã lưu" ghi rõ thư mục). Muốn lưu lại vào Pictures, chọn *Allow an app through Controlled folder access* rồi thêm `NeatShot.exe`, hoặc đổi thư mục trong Cài đặt.
- **Bấm `PrtSc` không có tác dụng:** Windows 11 có thể dành phím `PrtSc` cho Snipping Tool. Tắt tuỳ chọn *Settings → Accessibility → Keyboard → "Use the Print screen key to open screen capture"*, hoặc dùng `Ctrl + Shift + A`.
- **Báo "Không tìm thấy gói ngôn ngữ OCR":** thêm một ngôn ngữ có hỗ trợ OCR trong *Settings → Time & language → Language & region*.
- **Mất icon dưới khay hệ thống:** NeatShot tự gắn lại icon khi Explorer khởi động lại. Nếu vẫn không thấy, có thể ứng dụng đã thoát; hãy mở lại.

### Đóng góp

1. Tạo nhánh từ `main` (`feat/...`, `fix/...`, `docs/...`).
2. Làm theo TDD: viết test fail trước, sửa cho pass, rồi refactor.
3. Đảm bảo `dotnet test` pass trước khi mở PR.
4. Viết commit theo chuẩn [Conventional Commits](https://www.conventionalcommits.org/): `feat`, `fix`, `docs`, `test`, `chore`, `refactor`…

### Giấy phép

Phân phối theo giấy phép **MIT License**. Xem [LICENSE](./LICENSE).
