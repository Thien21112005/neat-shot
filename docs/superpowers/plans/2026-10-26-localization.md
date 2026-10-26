# Plan: Giao diện song ngữ (Tiếng Việt / English)

**Ngày:** 2026-10-26 · **Nhánh:** `feat/feedback-round-2`

## Mục tiêu
Toàn bộ chữ trên giao diện NeatShot hiển thị được bằng Tiếng Việt (mặc định) hoặc English; người dùng đổi trong
Cài đặt → Chung → Ngôn ngữ.

## Thiết kế
- `Common/Localization/Strings.cs`: **một bảng** `(Key, Vi, En)` cho mọi chuỗi — mỗi khóa luôn có đủ hai ngôn ngữ ở cùng một dòng.
- `Common/Localization/Loc.cs`:
  - `Loc.Language` ("vi" | "en"), `Loc.SetLanguage(choice)`, sự kiện `LanguageChanged`.
  - `Loc.T(key)` / `Loc.F(key, args)` dùng ngôn ngữ hiện tại; `Loc.Translate(key, language)` thuần để test.
  - Khóa không tồn tại → trả về chính khóa (dễ phát hiện), test chặn trường hợp này.
- `Common/Localization/TExtension.cs`: markup extension XAML `{l:T Settings.Title}`.
- `AppSettings.Language` = `"vi"` (mặc định) | `"en"`. App áp dụng khi khởi động và sau khi lưu Cài đặt
  (menu khay được dựng lại; cửa sổ mở sau đó dùng ngôn ngữ mới).
- Không dịch: dữ liệu sửa dấu OCR (`OcrTextPostProcessor`), log/Debug, thông báo exception nội bộ không hiện cho người dùng.

## Nhiệm vụ
1. [x] Loc + Strings + TExtension, test: chọn ngôn ngữ, bảng không trùng khóa / không thiếu bản dịch,
       mọi khóa dùng trong `src/**/*.xaml|cs` đều có trong bảng.
2. [x] Thay chuỗi cứng: tray/App, toolbar chụp, toolbar ghim, overlay (gợi ý, thẻ OCR, hộp thoại lưu),
       cửa sổ ghim, thư viện, cài đặt, hotkey box, countdown, toast, thông báo lỗi trong SettingsViewModel.
3. [x] Cài đặt: lựa chọn "Ngôn ngữ / Language" (Tiếng Việt | English), lưu `AppSettings.Language`.
4. [x] App: áp dụng ngôn ngữ khi khởi động và sau khi lưu; dựng lại menu khay.
5. [x] Gói MSIX khai báo thêm `en-US`; README, CHANGELOG, landing page.

## Tiêu chí hoàn thành
- Test bao phủ khóa xanh; toàn bộ test suite xanh.
- Chạy `inventory.py`: không còn chuỗi tiếng Việt hiển thị ngoài bảng `Strings.cs` (trừ dữ liệu OCR).
