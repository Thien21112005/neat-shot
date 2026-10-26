# NeatShot — Nội dung nộp Microsoft Store

Bản nháp để copy vào Partner Center khi bấm **Start submission**. Mỗi mục ghi rõ ô tương ứng trong Partner Center.

| Thông tin | Giá trị |
|---|---|
| Tên app | NeatShot |
| Loại sản phẩm | MSIX or PWA app |
| Package identity | `NguynNgcThin.NeatShot` · `CN=D3D8DD28-934A-4BF3-A370-B46154D78B04` |
| Nhà phát hành | Nguyễn Ngọc Thiện |
| Gói tải lên | `artifacts/msix/NeatShot-<version>.0-x64.msix` (tạo bằng `scripts/package-msix.ps1`) |
| Chính sách quyền riêng tư | https://thien21112005.github.io/neat-shot/privacy.html |
| Website | https://thien21112005.github.io/neat-shot/ |
| Hỗ trợ | https://github.com/Thien21112005/neat-shot/issues |

---

## 1. Pricing and availability

- **Markets**: tất cả thị trường.
- **Visibility**: Public.
- **Pricing**: Free.
- **Release**: As soon as it passes certification.

## 2. Properties

- **Category**: Productivity (phụ: Utilities & tools nếu được chọn).
- **Privacy policy URL**: https://thien21112005.github.io/neat-shot/privacy.html
- **Website**: https://thien21112005.github.io/neat-shot/
- **Support contact info**: https://github.com/Thien21112005/neat-shot/issues
- **System requirements** (Minimum): Windows 10 version 2004 (19041) trở lên, x64; Keyboard + Mouse.
- **Product declarations**: tick “This app has been tested to meet accessibility guidelines” chỉ khi đã tự kiểm tra; không tick các mục còn lại.

## 3. Age ratings (IARC)

Trả lời bảng câu hỏi: app thuộc nhóm **Utility / Productivity**, không có bạo lực, nội dung người lớn, cờ bạc, mua bán; **không** chia sẻ vị trí, **không** cho người dùng trò chuyện/tương tác với nhau, **không** thu thập dữ liệu cá nhân. Kết quả dự kiến: **3+ / Everyone**.

## 4. Packages

Tải `NeatShot-<version>.0-x64.msix` lên. Store tự ký gói, không cần chứng chỉ.

**Notes for certification** (ô "Submission options → Notes for certification"):

```
NeatShot is a desktop (WPF, full trust) screenshot tool. It runs in the system tray.
How to test: launch NeatShot, then press Ctrl+Shift+A (or PrtSc, or left-click the tray icon)
to open the capture overlay. Drag to select a region, then press Enter to copy.
runFullTrust is required to capture the screen, register global hotkeys and show a tray icon.
The windows.startupTask extension lets users keep NeatShot in the tray after sign-in;
it can be turned off from the tray menu, the app's Settings, or Windows Settings > Apps > Startup.
The app makes no network connections and collects no data.
```

**Restricted capability justification** (`runFullTrust`):

```
NeatShot is a Win32/WPF desktop app packaged with MSIX. Full trust is needed to capture
the screen (GDI), register system-wide hotkeys (RegisterHotKey) and show a notification
area icon (Shell_NotifyIcon).
```

---

## 5. Store listing — Tiếng Việt (vi-VN)

**Product name**: NeatShot

**Description**:

```
NeatShot là công cụ chụp màn hình gọn nhẹ cho Windows: chụp, chú thích, ghim lên màn hình và trích xuất chữ — tất cả bằng bàn phím, không cần tài khoản, hoàn toàn offline.

Nhấn Ctrl + Shift + A (hoặc PrtSc) ở bất kỳ đâu, kéo chọn vùng hoặc click để chọn nguyên một cửa sổ. Màn hình được "đóng băng" ngay lúc chụp, kể cả menu và tooltip đang mở, trên mọi màn hình kể cả khi khác DPI.

CHÚ THÍCH NGAY TRÊN ẢNH
Mũi tên, khung chữ nhật, elip, đường kẻ, bút, highlight, chữ, đánh số thứ tự, làm mờ và làm vỡ điểm ảnh để che thông tin nhạy cảm. Hoàn tác/làm lại không giới hạn, chọn màu bằng ống hút.

LÀM ĐẸP ẢNH
Thêm nền gradient, khoảng đệm, bo góc và đổ bóng mềm, xem trước trực tiếp — sẵn sàng để đăng lên mạng xã hội hay tài liệu.

GHIM LÊN MÀN HÌNH
Ghim ảnh vừa chụp hoặc ảnh trong Clipboard (F3) nổi trên mọi cửa sổ để đối chiếu khi làm việc. Lăn chuột để phóng to, Ctrl + lăn để chỉnh độ trong suốt.

TRÍCH XUẤT CHỮ (OCR) OFFLINE
Lấy chữ từ ảnh chụp bằng bộ OCR có sẵn của Windows, không gửi ảnh đi đâu cả.

TIỆN ÍCH HẰNG NGÀY
• Chụp hẹn giờ 3 / 5 / 10 giây để chụp menu và tooltip
• Chụp lại đúng vùng lần trước bằng Ctrl + Shift + R
• Đổi phím tắt, thư mục lưu, định dạng PNG/JPG trong Cài đặt
• Thư viện ảnh đã chụp, tự khởi động cùng Windows và chạy nền ở khay hệ thống

QUYỀN RIÊNG TƯ
NeatShot không kết nối mạng, không có quảng cáo, không thu thập dữ liệu. Ảnh và cài đặt chỉ nằm trên máy của bạn. Mã nguồn mở (MIT) trên GitHub.
```

**What's new in this version** (1.1.0):

```
• Cửa sổ Cài đặt: đổi phím tắt, thư mục lưu, PNG/JPG, tự copy khi chụp
• Tự nhận diện cửa sổ: rê chuột để thấy viền, click để chọn
• Ghim ảnh từ Clipboard bằng F3
• Chụp hẹn giờ 3 / 5 / 10 giây, giữ được menu và tooltip
• Chụp lại vùng vừa chụp bằng Ctrl + Shift + R
• Icon mới, thẻ OCR và bộ chọn Làm đẹp được thiết kế lại
```

**Product features** (mỗi dòng một ô, tối đa 20):

```
Chụp vùng, cửa sổ hoặc toàn màn hình bằng một phím tắt
Đóng băng màn hình — chụp được cả menu và tooltip
Hỗ trợ nhiều màn hình và DPI khác nhau
Mũi tên, khung, bút, highlight, chữ, đánh số, làm mờ
Làm đẹp ảnh: nền gradient, bo góc, đổ bóng
Ghim ảnh nổi trên mọi cửa sổ, ghim từ Clipboard
Trích xuất chữ (OCR) offline
Chụp hẹn giờ và chụp lại vùng cũ
Tuỳ chỉnh phím tắt, thư mục lưu, PNG/JPG
Không kết nối mạng, không quảng cáo, mã nguồn mở
```

**Search terms** (tối đa 7):

```
chụp màn hình
screenshot
snipping tool
OCR
chú thích ảnh
pin screenshot
lightshot
```

> Lưu ý: nếu Store từ chối từ khoá "lightshot" / "snipping tool" (tên sản phẩm khác), thay bằng "screen capture" và "annotate".

---

## 6. Store listing — English (en-US)

Gói khai báo cả `vi-VN` và `en-US` (giao diện app có Tiếng Việt và English), nên Partner Center sẽ yêu cầu điền listing cho cả hai ngôn ngữ.

**Description**:

```
NeatShot is a lightweight screenshot tool for Windows: capture, annotate, pin to screen and extract text — keyboard-first, no account, fully offline.

Press Ctrl + Shift + A (or PrtSc) anywhere, drag a region or click to grab a whole window. The screen is frozen the moment you capture, open menus and tooltips included, across every monitor even with mixed DPI.

ANNOTATE IN PLACE
Arrows, rectangles, ellipses, lines, pen, highlighter, text, numbered steps, plus blur and pixelate for sensitive data. Unlimited undo/redo and an eyedropper colour picker.

BEAUTIFY
Gradient backdrop, padding, rounded corners and a soft shadow with a live preview — ready for social posts and docs.

PIN TO SCREEN
Float a capture, or any image in the clipboard (F3), above every window for reference. Scroll to zoom, Ctrl + scroll for opacity.

OFFLINE TEXT EXTRACTION (OCR)
Copy text out of any capture using the OCR engine built into Windows. Images never leave your PC.

EVERYDAY TOOLS
• Delayed capture (3 / 5 / 10 s) for menus and tooltips
• Repeat the last region with Ctrl + Shift + R
• Custom hotkeys, save folder and PNG/JPG in Settings
• Capture gallery, starts with Windows and lives in the tray

PRIVACY
NeatShot makes no network connections, shows no ads and collects no data. Screenshots and settings stay on your PC. Open source (MIT) on GitHub.
```

**What's new in this version** (1.1.0):

```
• Settings window: hotkeys, save folder, PNG/JPG, copy on capture
• Window detection: hover to highlight, click to select
• Pin the clipboard image with F3
• Delayed capture (3 / 5 / 10 s) that keeps menus and tooltips
• Repeat the last region with Ctrl + Shift + R
• New app icon, redesigned OCR card and beautify picker
```

**Product features**:

```
Capture a region, window or full screen with one hotkey
Frozen screen — menus and tooltips stay in the shot
Multi-monitor and mixed-DPI support
Arrows, rectangles, pen, highlighter, text, steps, blur
Beautify: gradient backdrop, rounded corners, shadow
Pin captures or clipboard images above every window
Offline text extraction (OCR)
Delayed capture and repeat last region
Custom hotkeys, save folder and PNG/JPG
No network, no ads, open source
```

**Search terms**:

```
screenshot
screen capture
snipping tool
OCR
annotate
pin screenshot
lightshot
```

---

## 7. Hình ảnh cho trang Store

| Hạng mục | Kích thước | File / cách làm |
|---|---|---|
| Store logo (1:1) | 300 × 300 | `docs/store/StoreLogo-300.png` (sinh bởi `scripts/generate-icons.py`) |
| Screenshots (Desktop) | 1920 × 1080 (PNG, tối thiểu 1, nên 4–6 ảnh) | Chụp thật trên máy, xem gợi ý dưới |

Gợi ý bộ ảnh chụp (nền desktop gọn, màn hình 1920 × 1080, scale 100%):

1. Overlay đang chọn vùng trên một trang web, thanh công cụ hiện đầy đủ.
2. Ảnh có chú thích: mũi tên, khung đỏ, đánh số, làm mờ email.
3. Ảnh đã "Làm đẹp" với nền gradient và đổ bóng.
4. Hai ảnh đang được ghim trên màn hình cạnh cửa sổ code.
5. Thẻ OCR đang hiện chữ trích xuất.
6. Cửa sổ Cài đặt.

Có thể thêm một dòng chú thích ngắn cho mỗi ảnh (Caption, tối đa 200 ký tự).
