# NeatShot — Lộ trình tính năng (Roadmap)

> Cập nhật: 2026-10-26 · Phiên bản hiện tại: **v1.0.0** (nhánh `feat/landing-site` chuẩn bị cho v1.1.0)

Tài liệu này liệt kê các tính năng đề xuất cho NeatShot, xếp theo **giá trị cho người dùng so với công sức**.
Mỗi mục gồm: vấn đề cần giải quyết, phạm vi, tiêu chí hoàn thành và các phần code liên quan.

| Ký hiệu độ khó | Ý nghĩa |
|---|---|
| 🟢 Dễ | ≤ 1 ngày, chủ yếu tận dụng code sẵn có |
| 🟡 Vừa | 1–3 ngày, cần thêm service/UI mới |
| 🔴 Khó | > 3 ngày, cần nghiên cứu kỹ thuật hoặc thư viện ngoài |

---

## Tổng quan

| # | Tính năng | Nhóm | Độ khó | Ưu tiên |
|---|---|---|---|---|
| 1 | Cửa sổ Cài đặt + đổi phím tắt | Nền tảng | 🟡 | ⭐⭐⭐ |
| 2 | Tự nhận diện cửa sổ khi rê chuột | Chụp | 🟡 | ⭐⭐⭐ |
| 3 | Ghim ảnh từ Clipboard | Ghim | 🟢 | ⭐⭐⭐ |
| 4 | Chụp hẹn giờ (3s / 5s) | Chụp | 🟢 | ⭐⭐ |
| 5 | Chụp lại vùng vừa chụp | Chụp | 🟢 | ⭐⭐ |
| 6 | Kính lúp + toạ độ pixel khi chọn vùng | Chụp | 🟡 | ⭐⭐ |
| 7 | Công cụ chú thích mở rộng | Chú thích | 🟡 | ⭐⭐ |
| 8 | Đọc mã QR offline | Trích xuất | 🟡 | ⭐ |
| 9 | Hút màu toàn màn hình bằng phím tắt | Tiện ích | 🟢 | ⭐ |
| 10 | Quay màn hình GIF / MP4 | Ghi hình | 🔴 | ⭐⭐ |
| 11 | Chụp cuộn trang dài | Chụp | 🔴 | ⭐ |
| 12 | Bộ cài đặt, winget, ký số | Phát hành | 🟡 | ⭐⭐ |
| 13 | Kiểm tra bản cập nhật (tuỳ chọn) | Phát hành | 🟢 | ⭐ |

**Thứ tự đề xuất:** 1 → 2 → 3 → 4 → 5, sau đó chọn tiếp từ nhóm "Đáng làm tiếp".

---

## Giai đoạn 1 — Nên làm trước

### 1. Cửa sổ Cài đặt + đổi phím tắt 🟡

**Vấn đề:** `AppSettings` đã có `HotkeyCapture` và `AutoCopyOnCapture` nhưng chưa được dùng ở đâu. Phím tắt đang cố định
`Ctrl + Shift + A` / `PrtSc` trong `App.InitializeGlobalHotkeys()`. Người dùng không có chỗ nào để tuỳ chỉnh ngoài menu khay.

**Phạm vi:**
- Cửa sổ `SettingsWindow` (mở từ menu khay), cùng phong cách tối với toolbar/landing page.
- Nhóm **Phím tắt:** ô "bấm tổ hợp phím để gán", hiển thị dạng keycap; báo lỗi khi tổ hợp đã bị app khác chiếm (`RegisterHotKey` trả về `false`).
- Nhóm **Lưu ảnh:** chọn thư mục, bật/tắt tự lưu khi Copy/Ghim, định dạng PNG/JPG (+ chất lượng JPG).
- Nhóm **Chung:** khởi động cùng Windows, âm thanh khi chụp, tự sao chép sau khi chọn vùng (`AutoCopyOnCapture`).

**Tiêu chí hoàn thành:**
- [ ] Đổi phím tắt có hiệu lực ngay, không cần khởi động lại; lưu vào `settings.json`.
- [ ] Phím tắt bị trùng hiển thị cảnh báo và giữ lại phím cũ.
- [ ] Mọi tuỳ chọn trong `AppSettings` đều có UI tương ứng (không còn "cài đặt chết").
- [ ] Unit test cho việc parse/format chuỗi phím tắt (`"Ctrl+Shift+A"` ⇄ `Key` + `ModifierKeys`).

**Liên quan:** `Core/Models/AppSettings.cs`, `Core/Services/Implementations/HotkeyService.cs`, `App.xaml.cs`, `ScreenshotGalleryService`.

---

### 2. Tự nhận diện cửa sổ khi rê chuột 🟡

**Vấn đề:** Muốn chụp đúng một cửa sổ, người dùng phải kéo tay khớp từng mép. Snipaste/ShareX tự viền cửa sổ dưới con trỏ.

**Phạm vi:**
- Khi overlay mở và chưa có vùng chọn: rê chuột tới đâu thì cửa sổ (hoặc vùng con như panel, nút) dưới con trỏ được viền sẵn.
- Click một lần = chọn đúng vùng đó; kéo chuột = chọn tay như hiện tại.
- Liệt kê cửa sổ **trước khi** hiện overlay (`EnumWindows` + `DwmGetWindowAttribute(DWMWA_EXTENDED_FRAME_BOUNDS)` để bỏ phần bóng đổ).

**Tiêu chí hoàn thành:**
- [ ] Bỏ qua cửa sổ ẩn, thu nhỏ, cửa sổ của chính NeatShot.
- [ ] Toạ độ đúng trên đa màn hình và DPI hỗn hợp (100% / 125% / 150%).
- [ ] Unit test cho thuật toán chọn hình chữ nhật nhỏ nhất chứa con trỏ.

**Liên quan:** `Core/Interop/NativeMethods.cs`, `Presentation/Controls/SelectionCanvas.xaml.cs`, `Common/Helpers/DpiHelper.cs`.

---

### 3. Ghim ảnh từ Clipboard 🟢

**Vấn đề:** Ảnh copy từ trình duyệt, Zalo, Figma… không ghim lên màn hình được, trong khi `PinWindow` đã có sẵn.

**Phạm vi:**
- Phím tắt toàn cục (mặc định `F3`, đổi được trong Cài đặt) và mục "Ghim ảnh từ Clipboard" trong menu khay.
- Nếu Clipboard không có ảnh: hiện toast "Clipboard không có ảnh" (dùng `ToastWindow` sẵn có).

**Tiêu chí hoàn thành:**
- [ ] Ghim được ảnh PNG/Bitmap từ Clipboard, giữ đúng kích thước theo DPI.
- [ ] Ghim được nhiều ảnh liên tiếp.

**Liên quan:** `Presentation/Views/PinWindow.xaml.cs`, `Presentation/Views/ToastWindow.xaml.cs`, `HotkeyService`.

---

### 4. Chụp hẹn giờ (3s / 5s) 🟢

**Vấn đề:** Menu xổ xuống và tooltip biến mất khi bấm phím tắt nên không chụp được.

**Phạm vi:**
- Mục "Chụp sau 3 giây / 5 giây" trong menu khay.
- Đếm ngược nhỏ ở góc màn hình (không bị chụp vào ảnh).

**Tiêu chí hoàn thành:**
- [ ] Bộ đếm tự ẩn trước thời điểm `BitBlt`.
- [ ] Huỷ được bằng `Esc` trong lúc đếm.

**Liên quan:** `App.TriggerCapture()`, `ScreenCaptureService.CaptureCleanScreenAsync()`.

---

### 5. Chụp lại vùng vừa chụp 🟢

**Vấn đề:** Khi so sánh trước/sau (ví dụ sửa lỗi UI), phải kéo lại đúng vùng cũ nhiều lần.

**Phạm vi:**
- Lưu vùng chọn gần nhất (`CaptureRegion` + màn hình) vào settings.
- Phím tắt `Ctrl + Shift + R`: mở overlay với vùng cũ được chọn sẵn (tương tự "Chụp toàn màn hình" hiện tại).

**Tiêu chí hoàn thành:**
- [ ] Nếu bố cục màn hình thay đổi, vùng cũ được giới hạn lại trong màn hình ảo.
- [ ] Unit test cho việc clamp vùng chọn.

**Liên quan:** `OverlayWindow.SelectFullScreen()`, `AppSettings`, `CaptureRegion`.

---

## Giai đoạn 2 — Đáng làm tiếp

### 6. Kính lúp + toạ độ pixel khi chọn vùng 🟡
Phóng to ~8× vùng quanh con trỏ khi đang kéo chọn, hiện toạ độ `X, Y` và mã màu HEX, giúp chọn chính xác từng pixel.
Tận dụng `ColorPickerHelper` và kính lúp của công cụ hút màu hiện có.

### 7. Công cụ chú thích mở rộng 🟡
- **Spotlight:** làm tối toàn bộ ảnh trừ vùng quan trọng.
- **Chữ có nền** (nhãn nổi bật trên ảnh nhiều màu).
- **Kéo giãn / xoay** hình đã vẽ (hiện chỉ di chuyển được qua công cụ Chọn).
- Đổi độ dày nét bằng phím `[` / `]`.

Liên quan: `Core/Models/DrawingElement.cs`, `Common/Helpers/DrawingRenderer.cs`, `HitTestHelper.cs`, `DrawingCanvas.cs`.

### 8. Đọc mã QR offline 🟡
Nút "Đọc QR" trên toolbar: giải mã QR/Barcode trong vùng chọn, sao chép nội dung, mở link nếu là URL.
Cần thư viện giải mã offline (ví dụ ZXing.Net) — giữ đúng cam kết không kết nối mạng.

### 9. Hút màu toàn màn hình bằng phím tắt 🟢
Phím tắt mở thẳng chế độ hút màu (không cần chọn vùng), click để sao chép mã HEX/RGB, lưu vào lịch sử màu.

---

## Giai đoạn 3 — Lớn hơn, để sau

### 10. Quay màn hình GIF / MP4 🔴
Quay một vùng đã chọn, có nút dừng nổi, xuất GIF (cho bug report) hoặc MP4.
Cần nghiên cứu Windows.Graphics.Capture + bộ mã hoá; lưu ý hiệu năng và dung lượng file.

### 11. Chụp cuộn trang dài 🔴
Tự cuộn cửa sổ và ghép nhiều ảnh thành một ảnh dài. Khó làm ổn định với mọi ứng dụng (header dính, nội dung động).

### 12. Bộ cài đặt, winget, ký số 🟡
- Trình cài đặt (MSIX hoặc Inno Setup) tạo shortcut Start Menu, gỡ cài đặt sạch.
- Đăng ký **winget** (`winget install NeatShot`).
- **Ký số** file exe để bỏ cảnh báo SmartScreen (cần chứng chỉ code signing).

### 13. Kiểm tra bản cập nhật (tuỳ chọn) 🟢
Kiểm tra GitHub Releases khi người dùng **chủ động bật** trong Cài đặt — mặc định tắt để giữ đúng cam kết
"không kết nối mạng" trên landing page.

---

## Nguyên tắc khi triển khai

- **TDD:** viết test fail trước → sửa cho pass → refactor (xem `AGENTS.md`).
- **DPI & đa màn hình:** mọi tính năng chụp phải đúng trên 100% / 125% / 150%.
- **Offline:** không thêm tính năng gửi dữ liệu ra ngoài nếu người dùng chưa bật.
- **Giao diện:** dùng chung ngôn ngữ thiết kế với toolbar và landing page (icon vector nét mảnh, nền tối, accent `#0078D4`).
- **Commit:** mỗi tính năng một hoặc nhiều commit theo Conventional Commits (`feat(...)`, `fix(...)`, `test(...)`, `docs(...)`).
- Khi hoàn thành một tính năng: cập nhật `CHANGELOG.md`, README và (nếu phù hợp) landing page `site/`.
