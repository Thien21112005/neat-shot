# SRS: Ứng dụng chụp màn hình trên desktop

**Phiên bản:** 0.2
**Loại dự án:** Dự án cá nhân, phục vụ học tập
**Công nghệ lựa chọn:** C# (.NET 8 / .NET 9) + WPF (Windows Presentation Foundation)
**Tên dự án đề xuất:** NeatSnap (hoặc NeatShot)

---

## 1. Giới thiệu

### 1.1 Mục đích tài liệu
Tài liệu mô tả bài toán và các yêu cầu của một ứng dụng chụp màn hình trên máy tính (desktop). Tài liệu tập trung vào **cái gì cần làm**, chưa đề cập **làm bằng gì**.

### 1.2 Bối cảnh và vấn đề
Các công cụ chụp màn hình hiện có (ví dụ Lightshot) hoạt động theo cách: người dùng bấm phím tắt hoặc icon, màn hình "đóng băng", rồi kéo chọn vùng cần chụp. Trên Windows 11, cách này gặp một số vấn đề:

- Phím tắt (PrtSc) hay bị ứng dụng khác chiếm, không gọi được công cụ.
- Khi phải bấm icon trong popup khay hệ thống (tray) để chụp, popup đó vẫn còn mở và bị chụp dính vào ảnh.
- Người dùng khó kiểm soát hành vi của công cụ.

### 1.3 Mục tiêu
- Tạo một công cụ chụp màn hình gọn nhẹ, chụp **sạch** (không dính giao diện của chính công cụ hay popup hệ thống).
- Đồng thời là dự án để người làm học về lập trình ứng dụng desktop (phím tắt toàn cục, khay hệ thống, xử lý ảnh, clipboard).

### 1.4 Phạm vi
- **Giai đoạn 1 (Cốt lõi - MVP):** Chụp vùng chọn, chụp toàn màn hình, thanh công cụ vẽ chú thích nhanh trên ảnh tạm (bút vẽ tự do, vẽ khung chữ nhật, mũi tên, chọn màu, hoàn tác undo), lưu ảnh, sao chép vào clipboard, chạy nền dưới khay hệ thống, chụp "sạch" không dính popup.
- **Giai đoạn 2 (Tính năng đột phá - Killer Features):**
  1. *Pin to Screen (Ghim ảnh nổi):* Ghim vùng ảnh chụp nổi trên màn hình (Always on Top) để đối chiếu, học tập hoặc so sánh code.
  2. *Quick OCR (Trích xuất văn bản):* Quét chữ từ vùng ảnh chụp và copy text trực tiếp vào clipboard (offline).
  3. *Smart Blur / Pixelate:* Làm mờ hoặc che khảm điểm ảnh thông tin nhạy cảm (mật khẩu, token, thông tin cá nhân).
  4. *Step Counter:* Đánh số bước tự động (①, ②, ③...) phục vụ viết tài liệu/báo lỗi.
  5. *Beautify (Làm đẹp ảnh):* Tự động bo góc, đổ bóng mượt, thêm nền gradient nghệ thuật sẵn sàng chia sẻ.
  6. *Magnifier & Color Picker:* Kính lúp phóng to pixel và lấy mã màu HEX/RGB ngay khi chọn vùng.

**Ngoài phạm vi (bản đầu):** Quay video màn hình, đồng bộ đám mây, tài khoản người dùng, chia sẻ link online qua máy chủ bên thứ ba.

### 1.5 Đối tượng sử dụng
Người dùng Windows cần chụp màn hình nhanh hằng ngày (sinh viên, lập trình viên, người đi làm văn phòng). Ban đầu người dùng chính là chính tác giả.

### 1.6 Thuật ngữ

| Thuật ngữ | Ý nghĩa |
|---|---|
| Overlay | Lớp phủ toàn màn hình hiện ra để người dùng kéo chọn vùng |
| Tray | Khu vực icon ứng dụng chạy nền cạnh đồng hồ trên thanh taskbar |
| Phím tắt toàn cục | Phím tắt hoạt động kể cả khi ứng dụng không đang được focus |
| Vùng chọn | Hình chữ nhật người dùng kéo ra để chỉ định phần cần chụp |

---

## 2. Mô tả tổng quan

### 2.1 Góc nhìn sản phẩm
Ứng dụng độc lập, chạy nền trên Windows, không phụ thuộc dịch vụ bên ngoài. Người dùng gọi ứng dụng bằng phím tắt hoặc icon trên tray.

### 2.2 Luồng sử dụng chính
1. Ứng dụng chạy nền, có icon dưới tray.
2. Người dùng bấm phím tắt (hoặc icon tray).
3. Ứng dụng **ẩn mọi giao diện của chính nó**, chờ một khoảng ngắn để popup/menu đang mở biến mất, rồi mới chụp ảnh màn hình.
4. Ảnh chụp được hiển thị đóng băng toàn màn hình (overlay). Trong lúc rê chuột chọn vùng, kính lúp (Magnifier) hiển thị chi tiết pixel và mã màu HEX/RGB.
5. Người dùng kéo chuột chọn vùng cần lấy.
6. Thanh công cụ (toolbar) xuất hiện cạnh vùng chọn với các tính năng:
   - **Vẽ chú thích:** Khung chữ nhật, mũi tên, bút vẽ chì (pencil), đánh số bước tự động (① ② ③), chọn màu và kích thước nét vẽ.
   - **Bảo mật:** Bôi mờ (Blur) hoặc khảm điểm ảnh (Pixelate) che thông tin nhạy cảm.
   - **Thao tác nhanh:** Hoàn tác (Undo/Ctrl+Z).
7. Người dùng chọn hành động xuất:
   - **Sao chép / Lưu file:** Lưu hoặc copy vùng ảnh hoàn thiện vào clipboard.
   - **Ghim lên màn hình (Pin to Screen):** Đưa ảnh thành cửa sổ nổi Always-on-Top để vừa nhìn vừa làm việc.
   - **Quét chữ (OCR):** Tự động nhận diện chữ trong vùng chọn và đưa văn bản vào clipboard.
   - **Làm đẹp (Beautify):** Thêm khung bo góc, đổ bóng và nền gradient trước khi lưu/copy.
   - **Hủy:** Nhấn phím `Esc` để đóng overlay.

### 2.3 Ràng buộc và giả định
- Chạy trên Windows 10/11.
- Màn hình có thể dùng tỉ lệ hiển thị 100%, 125%, 150%, nên tọa độ chuột và kích thước ảnh có thể lệch nhau, ứng dụng phải xử lý đúng.
- Người dùng có thể dùng nhiều màn hình.

---

## 3. Yêu cầu chức năng

### 3.1 Chức năng cốt lõi (Core / MVP)

| Mã | Yêu cầu | Mức ưu tiên |
|---|---|---|
| FR-01 | Chụp màn hình bằng phím tắt toàn cục do người dùng cấu hình | Cao |
| FR-02 | Chụp màn hình từ icon trên tray | Cao |
| FR-03 | Trước khi chụp, ẩn giao diện của ứng dụng và đợi để popup/menu khác biến mất, đảm bảo ảnh không chứa các thành phần này | Cao |
| FR-04 | Hiển thị ảnh chụp đóng băng toàn màn hình để chọn vùng | Cao |
| FR-05 | Cho phép kéo chuột chọn vùng hình chữ nhật; phần ngoài vùng chọn được làm tối | Cao |
| FR-06 | Cho phép chỉnh lại vùng chọn (kéo cạnh, di chuyển) trước khi xác nhận | Trung bình |
| FR-07 | Thanh công cụ thao tác nhanh (Toolbar) nổi ngay cạnh vùng chọn | Cao |
| FR-08 | Công cụ bút vẽ tự do (Pencil / Freehand) vẽ trực tiếp lên ảnh tạm | Cao |
| FR-09 | Công cụ vẽ khung hình chữ nhật (Rectangle) để khoanh vùng nội dung quan trọng | Cao |
| FR-10 | Công cụ vẽ mũi tên (Arrow) và đường thẳng chỉ dẫn | Trung bình |
| FR-11 | Bảng chọn màu sắc nét vẽ (đỏ, xanh, vàng, trắng...) và tùy chỉnh độ dày nét | Cao |
| FR-12 | Tính năng hoàn tác nét vẽ (Undo / Ctrl+Z) trên ảnh tạm | Cao |
| FR-13 | Lưu vùng đã chọn (kèm các nét vẽ chú thích) thành file ảnh (PNG) | Cao |
| FR-14 | Sao chép vùng đã chọn (kèm các nét vẽ chú thích) vào clipboard | Cao |
| FR-15 | Hủy thao tác chụp bằng phím `Esc` | Cao |
| FR-16 | Chụp toàn màn hình không cần chọn vùng | Trung bình |
| FR-17 | Hỗ trợ nhiều màn hình | Trung bình |
| FR-18 | Cửa sổ cài đặt: đổi phím tắt, đổi thư mục lưu, bật/tắt khởi động cùng Windows | Trung bình |
| FR-19 | Chèn chữ (Text annotation) lên ảnh | Thấp |

### 3.2 Nhóm tính năng đột phá (Killer Features)

| Mã | Yêu cầu | Mức ưu tiên |
|---|---|---|
| FR-20 | **Ghim ảnh lên màn hình (Pin to Screen):** Tạo cửa sổ nổi không viền luôn nằm trên cùng (Always-on-Top), cho phép kéo thả, phóng to thu nhỏ, chỉnh độ mờ (opacity), đóng bằng phím `Esc` hoặc click đúp | Cao |
| FR-21 | **Nhận diện chữ tức thì (Quick OCR):** Trích xuất văn bản từ vùng ảnh chụp và copy text trực tiếp vào Clipboard (hoạt động offline qua Windows OCR API) | Cao |
| FR-22 | **Che mờ thông tin nhạy cảm (Smart Blur / Pixelate):** Công cụ bôi mờ (blur) hoặc khảm ô vuông (pixelate) che mật khẩu, token, thông tin riêng tư trước khi chia sẻ | Cao |
| FR-23 | **Đánh số bước tự động (Step Counter):** Mỗi lần click chuột tự động tạo nhãn số tròn tăng dần (①, ②, ③...) phục vụ viết tài liệu hướng dẫn hoặc báo cáo lỗi | Trung bình |
| FR-24 | **Làm đẹp ảnh tự động (Beautify):** Tự động thêm padding xung quanh, bo tròn góc, đổ bóng mềm (drop shadow) và nền màu gradient hiện đại | Trung bình |
| FR-25 | **Kính lúp & Bắt mã màu (Magnifier & Color Picker):** Hiển thị ô phóng to pixel x4 tại vị trí con trỏ khi chọn vùng, hiển thị mã màu HEX/RGB và cho phép copy nhanh mã màu | Trung bình |

---

## 4. Yêu cầu phi chức năng

| Mã | Yêu cầu |
|---|---|
| NFR-01 | **Tốc độ:** từ lúc bấm phím tắt đến lúc overlay hiện ra dưới 1 giây |
| NFR-02 | **Tài nguyên:** chạy nền chiếm ít RAM và gần như không dùng CPU khi nhàn rỗi |
| NFR-03 | **Chính xác:** ảnh xuất ra đúng từng pixel với vùng người dùng chọn, kể cả khi màn hình scale 125%/150% |
| NFR-04 | **Dễ dùng:** thao tác cơ bản (chụp, lưu/copy) hoàn thành trong tối đa 3 bước |
| NFR-05 | **Ổn định:** không treo hoặc kẹt overlay; luôn thoát được bằng `Esc` |
| NFR-06 | **Riêng tư:** ảnh chỉ lưu cục bộ, ứng dụng không gửi dữ liệu ra ngoài |

---

## 5. Ca sử dụng (Use case)

### UC-01: Chụp vùng và copy vào clipboard
- **Tác nhân:** Người dùng
- **Điều kiện đầu:** Ứng dụng đang chạy nền
- **Luồng chính:**
  1. Người dùng bấm phím tắt.
  2. Hệ thống ẩn giao diện của mình, chờ ngắn, chụp màn hình.
  3. Hệ thống hiện overlay đóng băng.
  4. Người dùng kéo chọn vùng.
  5. Người dùng xác nhận copy.
  6. Hệ thống đưa ảnh vào clipboard và đóng overlay.
- **Luồng thay thế:** Người dùng bấm `Esc` ở bước 4 hoặc 5 → hệ thống đóng overlay, không lưu gì.
- **Kết quả:** Ảnh vùng chọn nằm trong clipboard, không dính popup hay giao diện thừa.

### UC-02: Chụp từ icon tray khi popup đang mở
- **Mô tả:** Người dùng mở popup tray, bấm icon của ứng dụng.
- **Kỳ vọng:** Popup tray được đóng trước khi chụp, ảnh không chứa popup. *(Đây chính là lỗi của Lightshot cần khắc phục.)*

### UC-03: Đổi phím tắt
- Người dùng mở cài đặt, chọn tổ hợp phím mới. Hệ thống báo nếu phím bị trùng hoặc không đăng ký được.

### UC-04: Chụp vùng, vẽ chú thích (hình chữ nhật, bút vẽ) rồi copy/lưu
- **Tác nhân:** Người dùng
- **Điều kiện đầu:** Đã kéo chọn xong vùng trên màn hình overlay.
- **Luồng chính:**
  1. Thanh công cụ xuất hiện cạnh vùng chọn.
  2. Người dùng chọn công cụ vẽ khung hình chữ nhật, kéo thả để khoanh vùng điểm cần nhấn mạnh trên ảnh.
  3. Người dùng chọn công cụ bút chì (pencil), vẽ tự do để ghi chú/khoanh tròn.
  4. Nếu vẽ nhầm, bấm nút Undo hoặc phím tắt `Ctrl + Z` để hoàn tác nét vẽ gần nhất.
  5. Người dùng bấm nút Sao chép (hoặc Lưu file).
  6. Hệ thống kết xuất (render) ảnh bao gồm cả các lớp vẽ chú thích vào clipboard/file và đóng overlay.

### UC-05: Ghim ảnh nổi trên màn hình (Pin to Screen)
- **Tác nhân:** Người dùng (Lập trình viên, Designer, Sinh viên)
- **Luồng chính:**
  1. Người dùng kéo chọn vùng cần chụp.
  2. Bấm icon **Ghim (Pin)** trên thanh công cụ.
  3. Overlay biến mất; vùng ảnh chụp lập tức xuất hiện thành một cửa sổ nổi không viền luôn hiển thị trên cùng (Always-on-Top).
  4. Người dùng có thể kéo di chuyển cửa sổ, lăn chuột để phóng to/thu nhỏ, hoặc cuộn phím chỉnh độ trong suốt.
  5. Khi dùng xong, click đúp hoặc bấm `Esc` để đóng cửa sổ ghim.

### UC-06: Trích xuất chữ tức thì (Quick OCR)
- **Tác nhân:** Người dùng
- **Luồng chính:**
  1. Người dùng kéo chọn vùng chứa đoạn văn bản (trên video, ảnh tài liệu, web chống copy...).
  2. Bấm nút **OCR** trên thanh công cụ (hoặc phím tắt).
  3. Hệ thống quét nhận diện chữ (offline), hiển thị thông báo ngắn "Đã sao chép văn bản vào Clipboard".
  4. Người dùng có thể `Ctrl + V` nội dung text vào bất kỳ đâu ngay lập tức.

### UC-07: Che mờ thông tin nhạy cảm (Blur / Pixelate)
- **Tác nhân:** Người dùng
- **Luồng chính:**
  1. Người dùng kéo chọn công cụ **Blur** hoặc **Pixelate** trên toolbar.
  2. Kéo quét qua phần mật khẩu, số tài khoản hoặc hình ảnh riêng tư trên ảnh chụp tạm.
  3. Vùng chọn được làm mờ/khảm hạt mịn màng, không lộ nội dung gốc.
  4. Tiếp tục lưu hoặc sao chép ảnh đã được bảo mật.

---

## 6. Rủi ro và vấn đề cần làm rõ

| Vấn đề | Ghi chú |
|---|---|
| Phím tắt bị ứng dụng/hệ thống khác chiếm | Cần cơ chế báo lỗi và cho đổi phím |
| Scale màn hình khác nhau gây lệch tọa độ | Cần kiểm thử ở nhiều mức scale (100%, 125%, 150%) |
| Thời gian chờ để popup biến mất | Quá ngắn thì vẫn dính, quá dài thì chậm; cần thử nghiệm để chọn độ trễ tối ưu |
| Quản lý layer vẽ chú thích & Undo | Cần tách canvas vẽ trong suốt đè lên ảnh gốc để hỗ trợ Undo mượt mà |
| Quản lý nhiều cửa sổ ghim nổi (Pin) | Cần tối ưu tài nguyên tránh giật lag khi mở nhiều cửa sổ Pin cùng lúc |
| Hỗ trợ OCR tiếng Việt / tiếng Anh offline | Tận dụng sẵn API `Windows.Media.Ocr` của Windows để không phải nhúng model nặng |
| Hiệu năng xử lý bộ lọc ảnh (Blur/Mosaic) | Cần thuật toán xử lý mờ/khảm nhanh trực tiếp trên canvas pixel |
| Nhiều màn hình có độ phân giải khác nhau | Cần xác định cách ghép virtual screen coordinates |

---

## 7. Kế hoạch phát triển theo giai đoạn

1. **Mốc 1 (Cốt lõi màn hình):** Chụp toàn màn hình, lưu file, copy clipboard.
2. **Mốc 2 (Vùng chọn & Overlay):** Overlay đóng băng + kéo chọn vùng + thanh công cụ cơ bản.
3. **Mốc 3 (Dịch vụ chạy nền):** Tray icon + phím tắt toàn cục + xử lý ẩn popup trước khi chụp ("chụp sạch").
4. **Mốc 4 (Bộ công cụ vẽ chú thích):** Bút vẽ tự do, hình chữ nhật, mũi tên, chọn màu, Undo (`Ctrl+Z`).
5. **Mốc 5 (Đột phá - Đợt 1):** 
   - Ghim ảnh lên màn hình (Pin to Screen).
   - Che mờ thông tin nhạy cảm (Smart Blur / Pixelate).
   - Đánh số bước tự động (Step Counter ① ② ③).
6. **Mốc 6 (Đột phá - Đợt 2 & Hoàn thiện):** 
   - Quét chữ tức thì (Quick OCR offline qua Windows API).
   - Làm đẹp ảnh (Beautify: bo góc, đổ bóng shadow, nền gradient).
   - Kính lúp phóng to & bắt mã màu (Magnifier & Color Picker).
   - Cài đặt cấu hình, hỗ trợ đa màn hình.

---

## 8. Việc tiếp theo
- Chọn công nghệ phát triển (C# / .NET WPF / WinUI 3, Rust / Tauri, hoặc C++ / Qt).
- Thiết kế wireframe giao diện overlay và thanh công cụ tiện ích.
- Lập dự án khởi tạo (scaffold repository) và kiểm thử module chụp màn hình đầu tiên.
