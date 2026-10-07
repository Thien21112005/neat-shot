# NeatShot Phase 1 (Core / MVP) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Xây dựng ứng dụng chụp màn hình máy tính NeatShot gọn nhẹ chạy nền trên Windows, hỗ trợ phím tắt toàn cục, khay hệ thống (tray), thuật toán chụp sạch không dính popup rác, màn hình đóng băng toàn màn hình, kéo chọn vùng, vẽ chú thích vector (bút vẽ, hình chữ nhật, mũi tên, chọn màu, undo), lưu PNG và sao chép vào Clipboard.

**Architecture:** Clean Service Layer kết hợp mô hình MVVM (`CommunityToolkit.Mvvm`). Cấu trúc Solution phân tách `src/NeatShot` (WPF .NET 8) và `tests/NeatShot.Tests` (xUnit). Tầng `Core/Interop` giao tiếp Win32 API (`RegisterHotKey`, `SetForegroundWindow`, `BitBlt`) cho phím tắt và chụp ảnh đa màn hình Per-Monitor DPI v2.

**Tech Stack:** C# 12, .NET 8 (`net8.0-windows10.0.19041.0`), WPF, `CommunityToolkit.Mvvm` (8.x), `Wpf.Ui` (3.x), `H.NotifyIcon.Wpf` (2.x), `Microsoft.Extensions.DependencyInjection` (8.x), xUnit, FluentAssertions.

**Spec:** [SRS-screenshot-app.md](file:///d:/Tu-hoc/project/NeatShot/SRS-screenshot-app.md), [TECH-STACK-AND-ARCHITECTURE.md](file:///d:/Tu-hoc/project/NeatShot/TECH-STACK-AND-ARCHITECTURE.md)

---

## Global Constraints

- Target OS: Windows 10 (version 1809 trở lên) và Windows 11.
- Target Framework Moniker: `net8.0-windows10.0.19041.0`.
- Ứng dụng chạy nền mặc định: `ShutdownMode="OnExplicitShutdown"` trong `App.xaml`.
- DPI Awareness: `PerMonitorV2` được kích hoạt thông qua `app.manifest`.
- NFR-01: Thời gian từ khi kích hoạt chụp đến khi Overlay xuất hiện phải dưới 1 giây.
- NFR-03: Kích thước pixel và toạ độ vùng chọn xuất ra phải chuẩn xác 100% trên các tỉ lệ scale 100%, 125%, 150% và hệ thống đa màn hình.
- NFR-05: Luôn thoát được chế độ chụp an toàn bằng phím `Esc` mà không để treo tiến trình hay kẹt màn hình.

---

## Review Focus

1. **Hiển thị DPI Scale lệch:** Màn hình chính 125% DPI và màn hình phụ 100% DPI có thể làm lệch toạ độ chuột so với pixel ảnh chụp thực tế.
2. **Kéo chuột ngược chiều:** Người dùng kéo chọn vùng từ dưới lên trên hoặc từ phải sang trái (toạ độ width/height bị âm).
3. **Popup khay hệ thống còn sót lại:** Click icon trên Tray để chụp nhưng animation đóng menu của Windows chưa xong khiến menu bị chụp dính vào ảnh.
4. **Trùng phím tắt toàn cục:** Phím tắt (PrtSc hoặc phím tuỳ biến) đã bị ứng dụng khác hoặc hệ điều hành chiếm dụng khiến `RegisterHotKey` thất bại.
5. **Rò rỉ tài nguyên GDI Handle:** Sử dụng `BitBlt` và `GetDC` mà không gọi `DeleteObject` / `ReleaseDC` gây tràn bộ nhớ GDI objects sau nhiều lần chụp.

---

## Task Structure

### Task 1: Scaffolding Solution & Dự án WPF Clean Architecture

**Files:**
- Create: `NeatShot.sln`
- Create: `src/NeatShot/NeatShot.csproj`
- Create: `src/NeatShot/app.manifest`
- Create: `src/NeatShot/App.xaml`
- Create: `src/NeatShot/App.xaml.cs`
- Create: `tests/NeatShot.Tests/NeatShot.Tests.csproj`
- Create: `tests/NeatShot.Tests/SanityTests.cs`

**Interfaces:**
- Produces: Solution hoàn chỉnh với Dependency Injection container sẵn sàng đăng ký Services và ViewModels.

- [x] **Step 1: Tạo Solution và 2 Projects với .NET CLI**
  - Đã tạo `NeatShot.sln`, `src/NeatShot/NeatShot.csproj`, `tests/NeatShot.Tests/NeatShot.Tests.csproj`.
- [x] **Step 2: Thêm các NuGet packages cần thiết vào `src/NeatShot/NeatShot.csproj`**
  - Đã cài đặt `CommunityToolkit.Mvvm` (8.4.0), `WPF-UI` (4.3.0), `H.NotifyIcon.Wpf` (2.1.3), `Microsoft.Extensions.DependencyInjection` (10.0.2).
- [x] **Step 3: Cấu hình `app.manifest` kích hoạt DPI Awareness PerMonitorV2**
  - Đã thêm `app.manifest` với `PerMonitorV2`.
- [x] **Step 4: Cấu hình `App.xaml` và DI Container trong `App.xaml.cs`**
  - `ShutdownMode="OnExplicitShutdown"`, tích hợp `WPF-UI` Dark Theme, thiết lập ServiceProvider DI.
- [x] **Step 5: Viết Sanity Test và kiểm tra build toàn bộ Solution**
  - Chạy `dotnet test` -> Passed 1/1 test, 0 error, 0 warning.
- [x] **Step 6: Commit**
  - Đã khởi tạo Git và commit scaffold.

---

### Task 2: Models & Tiện ích tính toán toạ độ (DpiHelper, CaptureRegion)

**Files:**
- Create: `src/NeatShot/Core/Models/CaptureRegion.cs`
- Create: `src/NeatShot/Core/Models/DrawingElement.cs`
- Create: `src/NeatShot/Common/Helpers/DpiHelper.cs`
- Create: `src/NeatShot/Common/Helpers/ColorHelper.cs`
- Test: `tests/NeatShot.Tests/Helpers/DpiHelperTests.cs`
- Test: `tests/NeatShot.Tests/Models/CaptureRegionTests.cs`

**Interfaces:**
- Produces:
  - `struct CaptureRegion(double X, double Y, double Width, double Height)` kèm method `Normalize()` xử lý kéo ngược toạ độ.
  - `enum DrawingToolType { None, Pencil, Rectangle, Arrow, Text }`
  - `class DrawingElement` (ToolType, Color, Thickness, Points, Rect)
  - `DpiHelper.TransformToDevice(Point wpfPoint, double dpiScaleX, double dpiScaleY)`
  - `ColorHelper.ToHex(Color color)` và `ColorHelper.FromHex(string hex)`

- [x] **Step 1: Viết failing test cho `CaptureRegion.Normalize()`**
  - Đã viết unit test cho các trường hợp kéo xuôi và kéo ngược toạ độ cả 2 trục.
- [x] **Step 2: Chạy test kiểm tra FAIL**
  - Chạy `dotnet test --filter FullyQualifiedName~CaptureRegionTests` -> FAIL xác thực pha RED.
- [x] **Step 3: Triển khai `CaptureRegion.cs`**
  - Đã triển khai struct `CaptureRegion` với `Normalize()`, `ToRect()`, `FromPoints()`.
- [x] **Step 4: Viết test và triển khai `DpiHelper.cs` và `ColorHelper.cs`**
  - Đã viết test và triển khai `DpiHelper` (chuyển đổi toạ độ Per-Monitor DPI) và `ColorHelper` (HEX/RGB converter).
- [x] **Step 5: Chạy toàn bộ test để kiểm tra PASS**
  - Chạy `dotnet test` -> Passed 27/27 tests, 0 error, 0 warning.
- [x] **Step 6: Commit**
  - Đã commit theo Conventional Commit: `feat: implement CaptureRegion, DpiHelper, and ColorHelper with comprehensive unit tests`.

---

### Task 3: Tầng Interop Win32 & Dịch vụ Chụp ảnh màn hình (ScreenCaptureService)

**Files:**
- Create: `src/NeatShot/Core/Interop/NativeMethods.cs`
- Create: `src/NeatShot/Core/Interop/NativeStructs.cs`
- Create: `src/NeatShot/Core/Interop/NativeConstants.cs`
- Create: `src/NeatShot/Core/Services/Interfaces/IScreenCaptureService.cs`
- Create: `src/NeatShot/Core/Services/Implementations/ScreenCaptureService.cs`
- Test: `tests/NeatShot.Tests/Services/ScreenCaptureServiceTests.cs`

**Interfaces:**
- Produces:
  - `IScreenCaptureService`:
    ```csharp
    public interface IScreenCaptureService
    {
        Rect GetVirtualScreenBounds();
        Task<BitmapSource> CaptureCleanScreenAsync(CancellationToken cancellationToken = default);
        BitmapSource Crop(BitmapSource source, CaptureRegion region);
    }
    ```
  - `NativeMethods`: `BitBlt`, `GetDesktopWindow`, `SetForegroundWindow`, `ShowWindow`, `GetDC`, `ReleaseDC`.

- [ ] **Step 1: Định nghĩa Structs và P/Invoke trong `NativeMethods.cs`**
  - Định nghĩa `RECT`, `POINT`, hằng số Win32 (`SRCCOPY = 0x00CC0020`, `SW_HIDE = 0`).
  - Khai báo các hàm Win32 với `[DefaultDllImportSearchPaths]` và `SetLastError = true`.

- [ ] **Step 2: Viết failing test cho `ScreenCaptureService.Crop`**
  - Tạo một bitmap giả lập 500x500 trong memory, crop vùng 100x100 tại (50, 50) $\rightarrow$ kết quả phải có đúng kích thước 100x100.

- [ ] **Step 3: Triển khai `ScreenCaptureService.cs`**
  - `GetVirtualScreenBounds()`: Sử dụng `SystemParameters.VirtualScreenLeft`, `VirtualScreenTop`, `VirtualScreenWidth`, `VirtualScreenHeight`.
  - `CaptureCleanScreenAsync()`:
    1. Gửi tín hiệu ẩn giao diện, gọi `SetForegroundWindow(GetDesktopWindow())`.
    2. `await Task.Delay(120, cancellationToken)` để Windows hoàn tất đóng popup.
    3. Chụp bằng GDI `BitBlt` hoặc `Graphics.CopyFromScreen` bao phủ toàn bộ Virtual Screen.
    4. Chuyển đổi an toàn sang `BitmapSource` và đóng băng (`Freeze()`) để share giữa các thread.

- [ ] **Step 4: Chạy test kiểm tra crop và get virtual screen bounds**
  - Run: `dotnet test --filter FullyQualifiedName~ScreenCaptureServiceTests`
  - Expected: PASS.

- [ ] **Step 5: Commit**
  - Run:
    ```powershell
    git add src/NeatShot/Core/Interop/ src/NeatShot/Core/Services/ tests/NeatShot.Tests/
    git commit -m "feat: implement Win32 Interop and ScreenCaptureService with clean shot delay"
    ```

---

### Task 4: Quản lý Phím tắt Toàn cục & Khay hệ thống (Hotkey & Tray)

**Files:**
- Create: `src/NeatShot/Core/Services/Interfaces/IHotkeyService.cs`
- Create: `src/NeatShot/Core/Services/Implementations/HotkeyService.cs`
- Modify: `src/NeatShot/App.xaml`
- Modify: `src/NeatShot/App.xaml.cs`

**Interfaces:**
- Produces:
  - `IHotkeyService`:
    ```csharp
    public interface IHotkeyService : IDisposable
    {
        event EventHandler HotkeyPressed;
        bool Register(Key key, ModifierKeys modifiers);
        void Unregister();
    }
    ```

- [ ] **Step 1: Triển khai `HotkeyService.cs`**
  - Đăng ký `RegisterHotKey` Win32 API.
  - Sử dụng `HwndSource.FromHwnd` để gắn `AddHook` lắng nghe thông điệp `WM_HOTKEY (0x0312)`.
  - Kích hoạt sự kiện `HotkeyPressed` khi khớp mã phím.

- [ ] **Step 2: Cấu hình Tray Icon trong `App.xaml`**
  - Sử dụng `<tb:TaskbarIcon>` từ `H.NotifyIcon.Wpf`.
  - Khai báo ContextMenu: "Chụp vùng (PrtSc)", "Chụp toàn màn hình", "Cài đặt", "Thoát".
  - Gắn sự kiện `TrayIcon_TrayLeftMouseDown` gọi lệnh chụp màn hình.

- [ ] **Step 3: Đăng ký dịch vụ vào DI Container trong `App.xaml.cs`**
  - Đăng ký `IScreenCaptureService`, `IHotkeyService` dạng Singleton.
  - Trong `OnStartup`, khởi tạo HotkeyService với phím mặc định `PrintScreen` (hoặc phím phụ nếu PrtSc bị chiếm).

- [ ] **Step 4: Kiểm tra build và chạy thử**
  - Run: `dotnet build`
  - Expected: Build succeeded 0 warning, 0 error.

- [ ] **Step 5: Commit**
  - Run:
    ```powershell
    git add src/NeatShot/Core/Services/ src/NeatShot/App.xaml*
    git commit -m "feat: integrate global hotkey manager and system tray icon"
    ```

---

### Task 5: Màn hình Đóng băng (OverlayWindow) & Canvas kéo chọn vùng

**Files:**
- Create: `src/NeatShot/Presentation/Views/OverlayWindow.xaml`
- Create: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`
- Create: `src/NeatShot/Presentation/ViewModels/OverlayViewModel.cs`
- Create: `src/NeatShot/Presentation/Controls/SelectionCanvas.xaml`
- Create: `src/NeatShot/Presentation/Controls/SelectionCanvas.xaml.cs`

**Interfaces:**
- Consumes: `IScreenCaptureService.CaptureCleanScreenAsync()`
- Produces:
  - `OverlayWindow` bao phủ toạ độ ảo đa màn hình, hiển thị ảnh chụp đóng băng làm background.
  - `SelectionCanvas` cung cấp tương tác chuột kéo chọn hình chữ nhật:
    - Vùng ngoài được phủ màu đen mờ `rgba(0, 0, 0, 0.45)`.
    - Vùng trong trong suốt hiển thị ảnh chụp gốc.
    - Nhấn phím `Esc` $\rightarrow$ Đóng OverlayWindow.

- [ ] **Step 1: Tạo `OverlayViewModel.cs` với CommunityToolkit.Mvvm**
  - Thuộc tính `[ObservableProperty] BitmapSource? _backgroundImage`.
  - Thuộc tính `[ObservableProperty] CaptureRegion _selectedRegion`.
  - Lệnh `[RelayCommand] void Cancel()` $\rightarrow$ Đóng overlay.

- [ ] **Step 2: Thiết kế `OverlayWindow.xaml`**
  - `WindowStyle="None"`, `AllowsTransparency="True"`, `Topmost="True"`, `ShowInTaskbar="False"`.
  - Thiết lập `Left`, `Top`, `Width`, `Height` theo bounds của `VirtualScreen`.
  - Bắt sự kiện `KeyDown`: nếu bấm `Key.Escape` gọi `ViewModel.CancelCommand`.

- [ ] **Step 3: Triển khai UserControl `SelectionCanvas`**
  - Xử lý các sự kiện chuột `MouseDown`, `MouseMove`, `MouseUp`.
  - Tính toán toạ độ hình chữ nhật bằng `CaptureRegion.Normalize()`.
  - Sử dụng `Path` với `GeometryGroup` (kết hợp hình chữ nhật toàn màn hình và hình chữ nhật vùng chọn) để đục lỗ trong suốt hiển thị vùng chụp.

- [ ] **Step 4: Kết nối luồng Chụp từ Tray / Hotkey vào OverlayWindow**
  - Khi bấm chụp: `CaptureService.CaptureCleanScreenAsync()` $\rightarrow$ truyền ảnh vào `OverlayWindow` $\rightarrow$ `OverlayWindow.Show()`.

- [ ] **Step 5: Kiểm tra nghiệm thu thủ công và build**
  - Run: `dotnet build`
  - Expected: Build thành công.

- [ ] **Step 6: Commit**
  - Run:
    ```powershell
    git add src/NeatShot/Presentation/
    git commit -m "feat: implement fullscreen freeze OverlayWindow and SelectionCanvas"
    ```

---

### Task 6: Công cụ vẽ Chú thích Vector & Cơ chế Hoàn tác (Undo Stack)

**Files:**
- Create: `src/NeatShot/Core/Services/UndoStack.cs`
- Create: `src/NeatShot/Presentation/Controls/DrawingCanvas.cs`
- Create: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml`
- Create: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml.cs`
- Test: `tests/NeatShot.Tests/Services/UndoStackTests.cs`

**Interfaces:**
- Produces:
  - `UndoStack<T>`: Generic stack hỗ trợ `Push(T item)`, `T? Pop()`, `bool CanUndo`, `void Clear()`.
  - `DrawingCanvas`: Nhận các sự kiện vẽ (Rectangle, Arrow, Pencil), lưu vào danh sách `List<DrawingElement>`, vẽ lại trên `OnRender`.
  - `AnnotationToolbar`: Thanh công cụ nổi cạnh vùng chọn chứa các nút:
    - Bút vẽ tự do (Pencil)
    - Vẽ khung hình chữ nhật (Rectangle)
    - Mũi tên (Arrow)
    - Chọn màu sắc (Đỏ, Xanh, Vàng, Trắng) và kích thước nét vẽ
    - Nút Hoàn tác (Undo / Ctrl+Z)

- [ ] **Step 1: Viết failing test cho `UndoStack<T>`**
  - Test case: Push 3 phần tử $\rightarrow$ Pop ra đúng phần tử thứ 3 $\rightarrow$ CanUndo còn true $\rightarrow$ Pop tiếp 2 lần $\rightarrow$ CanUndo thành false.

- [ ] **Step 2: Triển khai `UndoStack.cs`**
  - Viết generic class `UndoStack<T>` dựa trên `Stack<T>`.

- [ ] **Step 3: Chạy test UndoStack**
  - Run: `dotnet test --filter FullyQualifiedName~UndoStackTests`
  - Expected: PASS.

- [ ] **Step 4: Triển khai `DrawingCanvas.cs` và Render nét vẽ**
  - Hỗ trợ công cụ vẽ: Bút chì (danh sách điểm Polyline), Hình chữ nhật, Mũi tên (đường thẳng kèm đầu tam giác).
  - Kết nối phím tắt `Ctrl + Z` để kích hoạt Undo từ `UndoStack`.

- [ ] **Step 5: Thiết kế `AnnotationToolbar.xaml`**
  - Nổi phía dưới (hoặc phía trên nếu sát cạnh đáy) của vùng chọn hình chữ nhật.
  - Sử dụng Style và Icon từ thư viện `Wpf.Ui`.

- [ ] **Step 6: Commit**
  - Run:
    ```powershell
    git add src/NeatShot/Core/Services/UndoStack.cs src/NeatShot/Presentation/Controls/ tests/NeatShot.Tests/
    git commit -m "feat: implement vector drawing canvas, undo stack, and annotation toolbar"
    ```

---

### Task 7: Xuất Ảnh (Lưu File PNG & Sao chép Clipboard) & Nghiệm thu MVP

**Files:**
- Create: `src/NeatShot/Core/Services/Interfaces/IExportService.cs`
- Create: `src/NeatShot/Core/Services/Implementations/ExportService.cs`
- Modify: `src/NeatShot/Presentation/ViewModels/OverlayViewModel.cs`
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml`
- Test: `tests/NeatShot.Tests/Services/ExportServiceTests.cs`

**Interfaces:**
- Produces:
  - `IExportService`:
    ```csharp
    public interface IExportService
    {
        RenderTargetBitmap RenderFinalImage(BitmapSource background, CaptureRegion region, IEnumerable<DrawingElement> annotations);
        Task CopyToClipboardAsync(BitmapSource image);
        Task SaveToFileAsync(BitmapSource image, string filePath);
    }
    ```

- [ ] **Step 1: Triển khai `ExportService.cs`**
  - `RenderFinalImage`: Sử dụng `RenderTargetBitmap` và `DrawingVisual` để ghép vùng ảnh crop gốc với toàn bộ nét vẽ vector, xuất ra ảnh PNG sắc nét đúng từng pixel.
  - `CopyToClipboardAsync`: Gọi `Clipboard.SetImage(image)` trên luồng STA Thread an toàn.
  - `SaveToFileAsync`: Sử dụng `PngBitmapEncoder` lưu file PNG xuống ổ cứng.

- [ ] **Step 2: Gắn lệnh vào Toolbar và Phím tắt**
  - Nút **Sao chép (Copy)** hoặc phím `Enter` / `Ctrl + C`: Xuất ảnh $\rightarrow$ Lưu vào Clipboard $\rightarrow$ Đóng Overlay $\rightarrow$ Hiện notification.
  - Nút **Lưu file (Save)** hoặc phím `Ctrl + S`: Bật `SaveFileDialog` $\rightarrow$ Lưu ảnh $\rightarrow$ Đóng Overlay.

- [ ] **Step 3: Chạy toàn bộ Unit Tests**
  - Run: `dotnet test`
  - Expected: PASS 100% tests.

- [ ] **Step 4: Chạy verification build toàn diện**
  - Run: `dotnet build --configuration Release`
  - Expected: 0 Warning, 0 Error, sinh ra file thực thi `NeatShot.exe`.

- [ ] **Step 5: Commit**
  - Run:
    ```powershell
    git add src/NeatShot/Core/Services/ src/NeatShot/Presentation/ tests/NeatShot.Tests/
    git commit -m "feat: implement image export, clipboard copying, and finalize MVP"
    ```
