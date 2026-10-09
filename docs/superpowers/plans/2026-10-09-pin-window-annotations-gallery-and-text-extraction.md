# Kế Hoạch Triển Khai: Đổi Nhãn Trích Xuất Chữ, Sửa Lỗi & Vẽ Trên Ảnh Ghim (Multi-Pin Annotation), và Quản Lý Thư Viện Ảnh NeatShot

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 
1. Đổi nhãn và hiển thị "OCR" thành "Trích xuất chữ" trên thanh công cụ và các thông báo nhằm thân thiện, dễ hiểu với người dùng.
2. Khắc phục triệt để lỗi click chuột trái làm cửa sổ ghim ảnh (`PinWindow`) bị đóng/biến mất; hỗ trợ ghim nhiều ảnh cùng lúc độc lập.
3. Bổ sung tính năng vẽ chú thích trực tiếp lên ảnh đã ghim (Bút vẽ, Hình chữ nhật, Mũi tên, Văn bản, Bước số) khi tương tác chuột trái vào hình ghim.
4. Tự động lưu và quản lý ảnh chụp vào thư mục `Pictures\NeatShot` (`C:\Users\<Username>\Pictures\NeatShot`).
5. Xây dựng giao diện Thư viện ảnh chụp (Screenshot Gallery Window) truy cập từ khay hệ thống, cho phép xem lịch sử và chọn ghim bất kỳ ảnh nào lên màn hình như ghi chú (sticky note).

**Architecture:**
- **Presentation / Controls / AnnotationToolbar & OverlayWindow:** Đổi nội dung hiển thị nút OCR thành "Trích xuất chữ", tooltip và tiêu đề card OCR kết quả.
- **Presentation / Views / PinWindow & ViewModels / PinViewModel:** 
  - Loại bỏ handler `MouseDoubleClick` đóng cửa sổ; bổ sung thanh điều khiển tiêu đề mini với nút đóng `✕`, nút ghim/bỏ ghim, và nút chuyển đổi chế độ chỉnh sửa/vẽ chú thích.
  - Tích hợp lớp vẽ `DrawingCanvas` ngay trên ảnh ghim trong `PinWindow`, kèm thanh công cụ mini (Mini Toolbar: Pen, Rect, Arrow, Text, Step, Color, Undo) xuất hiện khi click vào ảnh.
- **Core / Services / Gallery & Settings:**
  - Cập nhật mặc định `AppSettings.DefaultSaveDirectory` trỏ tới `Path.Combine(Environment.GetFolderPath(SpecialFolder.MyPictures), "NeatShot")`.
  - Bổ sung `IScreenshotGalleryService` phụ trách quét file ảnh, lấy metadata (thumbnail, dung lượng, ngày tạo) và xoá ảnh.
- **Presentation / Views / GalleryWindow & Tray Menu:**
  - Xây dựng cửa sổ `GalleryWindow` hiển thị lưới thumbnail các ảnh đã chụp trong thư mục `Pictures\NeatShot`.
  - Mỗi ảnh trong thư viện có nút thao tác nhanh: **Ghim lên màn hình (Pin)**, Sao chép (Copy), Mở file, Xoá file.
  - Thêm mục "Thư viện ảnh chụp" và "Mở thư mục ảnh NeatShot" vào menu chuột phải của System Tray Icon.

**Tech Stack:** C# 12, .NET 10 (`net10.0-windows10.0.19041.0`), WPF, XAML, xUnit.

**Spec:** [AGENTS.md](file:///d:/Tu-hoc/project/NeatShot/AGENTS.md), [TECH-STACK-AND-ARCHITECTURE.md](file:///d:/Tu-hoc/project/NeatShot/docs/TECH-STACK-AND-ARCHITECTURE.md).

---

## Global Constraints

- Target Framework: `net10.0-windows10.0.19041.0`.
- Thư mục ảnh mặc định chuẩn Windows: `Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)\NeatShot`.
- Các cửa sổ `PinWindow` phải hoạt động độc lập (`Topmost=True`), hỗ trợ mở nhiều cửa sổ cùng lúc không giới hạn mà không xung đột tài nguyên.
- Giữ nguyên toàn bộ 186 unit test hiện có đang pass; mọi tính năng mới phải có unit test bao phủ (luồng STA cho giao diện WPF).
- XAML tuân thủ phong cách Dark Fluent hiện đại (`#1E1E1E`, `#2D2D2D`, bo góc, drop shadow).

---

## Review Focus

1. **Click chuột trái vào PinWindow không được làm biến mất cửa sổ:** Thử nghiệm thao tác click đơn, click đúp hoặc kéo chuột không bao giờ kích hoạt lệnh Close. Cửa sổ chỉ đóng khi bấm phím Escape hoặc nút đóng `✕`.
2. **Kích hoạt công cụ vẽ trên PinWindow:** Khi bấm chuột trái hoặc chọn chế độ vẽ, thanh công cụ chú thích nổi lên cho phép vẽ các nét vector đè lên ảnh ghim, và khi hoàn thành có thể lưu lại ảnh đã chú thích.
3. **Thư mục Pictures\NeatShot tự động tạo lập:** Nếu thư mục chưa tồn tại, ứng dụng phải tự tạo an toàn mà không sinh ngoại lệ `DirectoryNotFoundException`.
4. **Hiệu năng GalleryWindow khi thư mục có nhiều ảnh:** Sử dụng `VirtualizingStackPanel` hoặc nạp thumbnail bất đồng bộ tránh làm đơ giao diện khi có hàng trăm ảnh chụp.
5. **Đồng bộ nhãn "Trích xuất chữ":** Đảm bảo mọi chuỗi giao diện hiển thị với người dùng đều thay thế chữ viết tắt OCR thành "Trích xuất chữ", phím tắt Ctrl + O vẫn giữ nguyên hoạt động.

---

## Task Structure

### Task 1: Đổi nhãn "OCR" thành "Trích xuất chữ" trên toàn bộ giao diện (UI Copywriting)

**Files:**
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`
- Test: `tests/NeatShot.Tests/Controls/AnnotationToolbarTests.cs`
- Test: `tests/NeatShot.Tests/Views/OverlayWindowLivePreviewAndOcrTests.cs`

**Interfaces:**
- Consumes: `Toolbar.OcrRequested`
- Produces: 
  - Nút trên Toolbar hiển thị Text: `"Trích xuất chữ"`, ToolTip: `"Trích xuất chữ từ ảnh (Ctrl + O)"`
  - Tiêu đề card kết quả: `"Kết quả Trích xuất chữ"`

- [x] **Step 1: Viết test kiểm tra nhãn hiển thị nút Trích xuất chữ trên Toolbar**
- [x] **Step 2: Chạy test kiểm tra trạng thái thất bại ban đầu (RED)**
- [x] **Step 3: Cập nhật XAML `AnnotationToolbar.xaml` và `OverlayWindow.xaml` sang "Trích xuất chữ"**
- [x] **Step 4: Chạy test xác nhận chuyển sang trạng thái thành công (GREEN)**
- [x] **Step 5: Commit `feat(ui): update OCR label and tooltip to user-friendly text extraction`**

---

### Task 2: Khắc phục lỗi đóng PinWindow khi click chuột trái & Bổ sung thanh điều khiển cửa sổ Pin

**Files:**
- Modify: `src/NeatShot/Presentation/Views/PinWindow.xaml`
- Modify: `src/NeatShot/Presentation/Views/PinWindow.xaml.cs`
- Modify: `src/NeatShot/Presentation/ViewModels/PinViewModel.cs`
- Test: `tests/NeatShot.Tests/ViewModels/PinViewModelTests.cs`
- Test: `tests/NeatShot.Tests/Views/PinWindowInteractionTests.cs`

**Interfaces:**
- Consumes: `PinViewModel.RequestClose`, `PinViewModel.Opacity`
- Produces:
  - Loại bỏ `MouseDoubleClick="OnMouseDoubleClick"` khỏi `PinWindow.xaml`
  - Thêm header bar ẩn/hiện khi hover chứa: Nút đóng `✕`, Nút sao chép `📋`, Nút bật/tắt vẽ chú thích `✏️`
  - Click chuột trái vào ảnh ghim sẽ không bị đóng hay biến mất.

- [ ] **Step 1: Viết test xác nhận click chuột đơn/đúp vào PinWindow không kích hoạt RequestClose**
- [ ] **Step 2: Chạy test xác minh lỗi thất bại ban đầu**
- [ ] **Step 3: Loại bỏ sự kiện `MouseDoubleClick` gọi `Close()` và xây dựng thanh header mini với nút `✕`**
- [ ] **Step 4: Chạy test xác nhận test chuyển sang GREEN**
- [ ] **Step 5: Commit `fix(pin): prevent accidental close on left click and add mini header controls`**

---

### Task 3: Bổ sung công cụ vẽ chú thích trực tiếp lên ảnh đã ghim (Multi-Pin Live Annotation)

**Files:**
- Create: `src/NeatShot/Presentation/Controls/PinAnnotationToolbar.xaml`
- Create: `src/NeatShot/Presentation/Controls/PinAnnotationToolbar.xaml.cs`
- Modify: `src/NeatShot/Presentation/Views/PinWindow.xaml`
- Modify: `src/NeatShot/Presentation/Views/PinWindow.xaml.cs`
- Test: `tests/NeatShot.Tests/Views/PinWindowAnnotationTests.cs`

**Interfaces:**
- Consumes: `DrawingCanvas`, `DrawingToolType`, `UndoStack`
- Produces:
  - `PinWindow` chứa `DrawingCanvas` lồng trên ảnh ghim.
  - `PinAnnotationToolbar`: Thanh công cụ mini nổi (Pencil, Rectangle, Arrow, Text, StepCounter, Color, Undo, Close).
  - Khi người dùng click chuột trái vào ảnh hoặc click nút `✏️`, thanh công cụ vẽ hiển thị cho phép vẽ chú thích trực tiếp.

- [ ] **Step 1: Viết unit test cho tính năng vẽ chú thích và undo trên PinWindow**
- [ ] **Step 2: Chạy test xác nhận RED**
- [ ] **Step 3: Tạo `PinAnnotationToolbar` và lồng `DrawingCanvas` vào `PinWindow.xaml`**
- [ ] **Step 4: Kết nối sự kiện vẽ, chọn màu, chọn công cụ trong `PinWindow.xaml.cs`**
- [ ] **Step 5: Chạy test xác nhận GREEN**
- [ ] **Step 6: Commit `feat(pin): add live vector annotations and sticky note editing to PinWindow`**

---

### Task 4: Tự động lưu & Quản lý thư mục ảnh `Pictures\NeatShot` (Default Storage Service)

**Files:**
- Create: `src/NeatShot/Core/Services/Interfaces/IScreenshotGalleryService.cs`
- Create: `src/NeatShot/Core/Services/Implementations/ScreenshotGalleryService.cs`
- Create: `src/NeatShot/Core/Models/GalleryItem.cs`
- Modify: `src/NeatShot/Core/Services/Implementations/SettingsService.cs`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`
- Test: `tests/NeatShot.Tests/Services/ScreenshotGalleryServiceTests.cs`

**Interfaces:**
- Consumes: `Environment.SpecialFolder.MyPictures`, `AppSettings.DefaultSaveDirectory`
- Produces:
  - `IScreenshotGalleryService.GetScreenshotsDirectory(): string` (trả về `C:\Users\<User>\Pictures\NeatShot`, tự tạo nếu chưa có)
  - `IScreenshotGalleryService.GetSavedScreenshotsAsync(): Task<IReadOnlyList<GalleryItem>>`
  - `IScreenshotGalleryService.SaveScreenshotAsync(BitmapSource image, string? filename = null): Task<string>`
  - `IScreenshotGalleryService.DeleteScreenshot(string filePath): bool`

- [ ] **Step 1: Viết test cho ScreenshotGalleryService kiểm tra đường dẫn mặc định và lưu file**
- [ ] **Step 2: Chạy test xác nhận RED**
- [ ] **Step 3: Triển khai `GalleryItem`, `IScreenshotGalleryService` và `ScreenshotGalleryService`**
- [ ] **Step 4: Cập nhật `SettingsService` để `DefaultSaveDirectory` mặc định luôn là `Pictures\NeatShot`**
- [ ] **Step 5: Chạy test xác nhận GREEN**
- [ ] **Step 6: Commit `feat(storage): setup default Pictures\NeatShot directory and ScreenshotGalleryService`**

---

### Task 5: Xây dựng giao diện Thư viện ảnh (Screenshot Gallery Window) & Ghim ảnh bất kỳ từ kho

**Files:**
- Create: `src/NeatShot/Presentation/ViewModels/GalleryViewModel.cs`
- Create: `src/NeatShot/Presentation/Views/GalleryWindow.xaml`
- Create: `src/NeatShot/Presentation/Views/GalleryWindow.xaml.cs`
- Modify: `src/NeatShot/App.xaml.cs`
- Test: `tests/NeatShot.Tests/ViewModels/GalleryViewModelTests.cs`
- Test: `tests/NeatShot.Tests/Views/GalleryWindowTests.cs`

**Interfaces:**
- Consumes: `IScreenshotGalleryService`, `PinWindow`
- Produces:
  - `GalleryWindow`: Cửa sổ quản lý danh sách ảnh chụp với giao diện thẻ/thumbnail hiện đại, hiển thị thời gian, kích thước.
  - Nút **"Ghim lên màn hình" (Pin)** trên mỗi ảnh trong thư viện -> mở một `PinWindow` độc lập với ảnh đó.
  - Nút "Mở thư mục" -> mở `explorer.exe C:\Users\<User>\Pictures\NeatShot`.
  - Tích hợp mục "Thư viện ảnh chụp" và "Mở thư mục ảnh NeatShot" trong menu khay hệ thống `App.xaml.cs`.

- [ ] **Step 1: Viết test cho GalleryViewModel (tải danh sách ảnh, lệnh Ghim ảnh, lệnh Xoá ảnh)**
- [ ] **Step 2: Chạy test xác nhận RED**
- [ ] **Step 3: Xây dựng `GalleryViewModel` và XAML giao diện `GalleryWindow` với Fluent Dark theme**
- [ ] **Step 4: Bổ sung lệnh mở GalleryWindow vào menu khay hệ thống trong `App.xaml.cs`**
- [ ] **Step 5: Chạy toàn bộ kiểm thử xác nhận GREEN**
- [ ] **Step 6: Commit `feat(gallery): add screenshot gallery window with sticky note pin-to-screen feature`**

---

## Verification Plan

### Automated Tests:
- Chạy toàn bộ bộ test suite:
  ```powershell
  dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj
  ```
- Kỳ vọng: Toàn bộ 186 test hiện hữu cùng các test mới bổ sung đều vượt qua 100% (Failed: 0).

### Manual Verification:
1. Mở NeatShot (`dotnet run --project src/NeatShot/NeatShot.csproj`), nhấn `Ctrl + Shift + A`.
2. Kiểm tra thanh công cụ: Nút OCR đã đổi thành "Trích xuất chữ", hover hiển thị tooltip rõ ràng.
3. Chọn vùng và bấm nút "Ghim" (Pin):
   - Thử click chuột trái lên hình ghim: Ảnh KHÔNG bị biến mất.
   - Thử click nút vẽ `✏️` hoặc click vào ảnh: Thanh công cụ vẽ xuất hiện, vẽ thử hình chữ nhật/mũi tên/text lên ảnh ghim.
   - Ghim thêm 2-3 ảnh khác để kiểm tra tính năng ghim nhiều ảnh cùng lúc độc lập.
4. Mở khay hệ thống, click chuột phải chọn "Thư viện ảnh chụp":
   - Cửa sổ Thư viện ảnh mở ra với các ảnh trong `C:\Users\<Username>\Pictures\NeatShot`.
   - Bấm nút "Ghim lên màn hình" trên một ảnh cũ trong thư viện: Ảnh đó lập tức ghim nổi lên màn hình như một sticky note.
