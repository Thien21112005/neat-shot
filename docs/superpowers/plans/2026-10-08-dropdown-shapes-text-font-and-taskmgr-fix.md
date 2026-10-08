# Kế Hoạch Triển Khai: Ô Thả Xuống Hình Khối, Tùy Chọn Phông Chữ Text & Sửa Lỗi Task Manager

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 
1. Khắc phục triệt để lỗi mở Task Manager ngoài ý muốn khi nhấn phím tắt `Ctrl + Shift + A` chụp màn hình.
2. Gộp công cụ Hình chữ nhật & Oval (Ellipse) vào một ô thả xuống (Dropdown/ComboBox), và Đường thẳng & Mũi tên (Line & Arrow) vào một ô thả xuống trên thanh công cụ.
3. Bổ sung giao diện chọn phông chữ (Font Family) và cỡ chữ (Font Size) cho công cụ chèn văn bản (Text Annotation).

**Architecture:** 
- **Core / ScreenCaptureService:** Loại bỏ việc gọi giả lập phím `VK_ESCAPE` qua `keybd_event` (nguyên nhân gây xung đột tổ hợp `Ctrl + Shift + Escape`). Chuyển sang Win32 `NativeMethods.SetForegroundWindow(NativeMethods.GetDesktopWindow())` để đóng popup/menu hệ thống an toàn, đúng chuẩn Clean Shot của tài liệu kiến trúc.
- **Presentation / Controls / AnnotationToolbar:** Tái cấu trúc thanh công cụ:
  - Thay thế 4 nút riêng lẻ (Rect, Ellipse, Line, Arrow) bằng 2 ô drop box thả xuống: `ShapeComboBox` (Hình chữ nhật / Oval) và `LineComboBox` (Đường thẳng / Mũi tên) với phong cách Fluent Dark Design, vẫn đồng bộ phím tắt `R`, `O`, `L`, `A`.
  - Bổ sung bộ điều khiển phông chữ `FontFamilyComboBox` và `FontSizeComboBox` khi dùng công cụ Text, liên kết với `DrawingCanvas.CurrentFontFamily` và `DrawingCanvas.CurrentFontSize`.
- **Testing:** Kiểm thử đơn vị toàn diện với xUnit trên luồng STA cho các thao tác chọn drop box và phông chữ, bảo đảm toàn bộ test suite tiếp tục xanh 100%.

**Tech Stack:** C# 12, .NET 10 (`net10.0-windows10.0.19041.0`), WPF, WPF-UI, xUnit.

**Spec:** [SRS-screenshot-app.md](file:///d:/Tu-hoc/project/NeatShot/docs/SRS-screenshot-app.md) (FR-07, FR-09, FR-10, FR-19), [TECH-STACK-AND-ARCHITECTURE.md](file:///d:/Tu-hoc/project/NeatShot/docs/TECH-STACK-AND-ARCHITECTURE.md) (Mục 4.1 & 4.3).

---

## Global Constraints

- Target OS: Windows 10 (1809 trở lên) và Windows 11.
- Target Framework: `net10.0-windows10.0.19041.0`.
- Tuyệt đối không gửi `keybd_event` khi các phím bổ trợ (`Ctrl`, `Shift`, `Alt`, `Win`) đang được nhấn giữ.
- XAML controls tuân thủ Dark Theme đồng bộ của NeatShot (`#E6242424`, `#33FFFFFF`).
- Mọi thao tác cập nhật UI hoặc Dispatcher chạy đúng trên luồng STA.
- Luôn chạy `dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj` sau mỗi thay đổi và không được làm gãy bất kỳ bài test nào.

---

## Review Focus

1. **Xung đột phím tắt hệ thống (Ctrl + Shift + Escape):** Tránh bất kỳ lệnh nào phát sinh tổ hợp phím kích hoạt Windows Shell hoặc Task Manager khi người dùng đang giữ phím tắt chụp.
2. **Đồng bộ hai chiều giữa Dropdown và Phím tắt:** Khi bấm phím tắt `R`, `O`, `L`, `A` trên bàn phím, ô thả xuống tương ứng phải tự động cập nhật mục đang chọn.
3. **Hiển thị phông chữ có sẵn:** Danh sách phông chữ hiển thị phải là các phông tiêu chuẩn luôn có trên Windows (Segoe UI, Arial, Calibri, Consolas, Times New Roman, Verdana, Tahoma).
4. **Cỡ chữ hợp lệ:** Giá trị FontSize phải luôn dương và được giới hạn hợp lý (ví dụ: 12px đến 48px).
5. **Cập nhật động phần tử đang chọn:** Khi dùng công cụ Select chọn một dòng Text đã vẽ trước đó, việc thay đổi Font Family hoặc Font Size trên thanh công cụ sẽ cập nhật ngay đối tượng đó.

---

## Task Structure

### Task 1: Khắc phục lỗi Task Manager tự bật trong ScreenCaptureService (Bugfix & Root Cause Fix)

**Files:**
- Modify: `src/NeatShot/Core/Services/Implementations/ScreenCaptureService.cs`
- Modify: `src/NeatShot/Core/Interop/NativeMethods.cs`
- Test: `tests/NeatShot.Tests/Services/ScreenCaptureServiceTests.cs`

**Interfaces:**
- Consumes: `NativeMethods.SetForegroundWindow`, `NativeMethods.GetDesktopWindow`
- Produces: `ScreenCaptureService.CaptureCleanScreenAsync` an toàn tuyệt đối, không phát sinh phím `VK_ESCAPE`.

- [x] **Step 1: Viết test xác nhận ScreenCaptureService chạy an toàn không phát phím Escape**
- [x] **Step 2: Loại bỏ `keybd_event(VK_ESCAPE)` và dùng `SetForegroundWindow(GetDesktopWindow())`**
- [x] **Step 3: Chạy test kiểm chứng kết quả qua `dotnet test`**

---

### Task 2: Gộp Hình chữ nhật & Oval, Đường thẳng & Mũi tên vào ô thả xuống (Shape & Line Dropdown Boxes)

**Files:**
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml`
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml.cs`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`
- Modify: `tests/NeatShot.Tests/Controls/AnnotationToolbarTests.cs`

**Interfaces:**
- Consumes: `DrawingToolType.Rectangle`, `DrawingToolType.Ellipse`, `DrawingToolType.Line`, `DrawingToolType.Arrow`
- Produces:
  - `AnnotationToolbar.ShapeComboBox`: Dropdown chọn Hình chữ nhật (R) hoặc Oval (O)
  - `AnnotationToolbar.LineComboBox`: Dropdown chọn Đường thẳng (L) hoặc Mũi tên (A)
  - Đồng bộ lựa chọn với `ToolSelected` event và các phím tắt bàn phím.

- [x] **Step 1: Viết test cho sự kiện chọn tool từ ô thả xuống Shape và Line**
- [x] **Step 2: Cập nhật XAML và Code-behind của `AnnotationToolbar`**
- [x] **Step 3: Đồng bộ trạng thái phím tắt trong `OverlayWindow.xaml.cs`**
- [x] **Step 4: Chạy `dotnet test` xác nhận kiểm thử thành công**

---

### Task 3: Bổ sung ô chọn phông chữ và cỡ chữ cho công cụ Text (Font Family & Font Size Selector)

**Files:**
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml`
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml.cs`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`
- Modify: `src/NeatShot/Presentation/Controls/DrawingCanvas.cs`
- Test: `tests/NeatShot.Tests/Controls/AnnotationToolbarTests.cs`
- Test: `tests/NeatShot.Tests/Controls/DrawingCanvasTextTests.cs`

**Interfaces:**
- Consumes: `DrawingCanvas.CurrentFontFamily`, `DrawingCanvas.CurrentFontSize`
- Produces:
  - `AnnotationToolbar.FontFamilyComboBox`: Chọn phông chữ (Segoe UI, Arial, Consolas...)
  - `AnnotationToolbar.FontSizeComboBox`: Chọn cỡ chữ (12, 14, 16, 20, 24, 32...)
  - `AnnotationToolbar.FontFamilyChanged`, `AnnotationToolbar.FontSizeChanged` events
  - Tự động hiển thị/kích hoạt bảng phông chữ khi công cụ Text đang hoạt động.

- [x] **Step 1: Viết test cho sự kiện thay đổi phông chữ và cỡ chữ trên Toolbar**
- [x] **Step 2: Thêm giao diện ComboBox chọn phông chữ và cỡ chữ vào `AnnotationToolbar.xaml`**
- [x] **Step 3: Kết nối sự kiện thay đổi phông chữ giữa `AnnotationToolbar` và `DrawingCanvas` trong `OverlayWindow.xaml.cs`**
- [x] **Step 4: Cập nhật font động cho Text element đang được chọn**
- [x] **Step 5: Chạy `dotnet test` xác nhận toàn bộ 100+ tests pass**
