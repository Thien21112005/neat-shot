# Advanced Annotations & Custom Color History Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mo rong bo cong cu chu thich tren NeatShot voi cac tinh nang: ve oval (elip), ve duong thang (line), di chuyen cac hinh da ve (select & move), but da quang (highlight ban trong suot), nhap text vao khung chu (inline text annotation), cay but hut mau he thong (eyedropper / screen color picker) voi kinh lup phong to pixel, va khung luu lich su mau chon (color history palette).

**Architecture:** Giu vung kien truc Vector Canvas va Clean Service Layer cua NeatShot. Mo rong `DrawingElement` va `DrawingToolType` trong Core Models. Mo rong `DrawingRenderer` trong Common Helpers de ve them Ellipse, Line, Highlight, va FormattedText (dung chung giua `DrawingCanvas` va `ExportService`). Bo sung `HitTestHelper` phuc vu lua chon va keo tha di chuyen hinh ve. Bo sung `ColorPickerHelper` trich xuat mau pixel chinh xac tren nen DPI scaling, kem `ColorHistory` quan ly danh sach mau gan day tren `AnnotationToolbar`.

**Tech Stack:** C# 12, .NET 10 (`net10.0-windows10.0.19041.0`), WPF, `CommunityToolkit.Mvvm` (8.4.0), `WPF-UI` (4.3.0), xUnit.

**Spec:** [SRS-screenshot-app.md](file:///d:/Tu-hoc/project/NeatShot/docs/SRS-screenshot-app.md) (FR-10, FR-11, FR-19, FR-25), [TECH-STACK-AND-ARCHITECTURE.md](file:///d:/Tu-hoc/project/NeatShot/docs/TECH-STACK-AND-ARCHITECTURE.md) (Muc 4.3)

---

## Global Constraints

- Target OS: Windows 10 (version 1809 tro len) va Windows 11.
- Target Framework Moniker: `net10.0-windows10.0.19041.0`.
- Dung chuan WPF Vector Canvas: khong ghi de pha huy bitmap goc, tat ca hinh ve va text phai luu duoi dang `DrawingElement` trong `UndoStack` de Undo/Redo hoat dong tron ven.
- Xu ly chuan xac DPI Scaling tren man hinh 100%, 125%, 150%: toa do lay mau mau tu `BitmapSource` phai chuyen doi tuong thich giua DIP (Device Independent Pixels) va toa do pixel vat ly.
- Export hinh anh cuoi cung qua `ExportService.RenderFinalImage` phai render dong nhat voi nhung gi nguoi dung thay tren man hinh overlay.
- Khong them bat ky tap tin CI vao thu muc `.github/`. Khong su dung emoji trong tai lieu markdown.

---

## Review Focus

1. **Loi chia 0 hoac Rect am khi ve Oval va Line nguoc huong:** Nguoi dung keo chuot tu phai sang trai hoac tu duoi len tren khien Width/Height bi am, can chuan hoa qua `Math.Min` va `Math.Abs`.
2. **Text nhap rong hoac bo dang do (Escape / blur):** Nguoi dung click de nhap text nhung khong go gi hoac nhan Escape khong duoc tao `DrawingElement` rac vao `UndoStack`.
3. **Lec toa do pixel khi hut mau tren man hinh scale DPI 125% / 150%:** Vi tri chuot la DIP nhung `BitmapSource.CopyPixels` nhan pixel thuc te; can nhan ti le scale tuong ung de khong lay lech mau.
4. **Hit-test nham hoac khong the chon hinh ve nho:** Nguoi dung click vao duong vien oval hoac duong thang co do day mong; can co nguong dung sai `tolerance` (khoang 6-8px) de de dang click chon va di chuyen.
5. **Trung lap va tran bo nho trong Lich su mau (Color History):** Khi hut nhieu lan cung mot mau hoac hang chuc mau khac nhau, lich su can gioi han dung luong (vi du toi da 8 mau) va day mau moi len dau (LRU) ma khong bi duplicate.

---

## Task Structure

### Task 1: Mo rong Models va Hit-Test Helper

**Files:**
- Modify: `src/NeatShot/Core/Models/DrawingElement.cs`
- Create: `src/NeatShot/Common/Helpers/HitTestHelper.cs`
- Create: `tests/NeatShot.Tests/Helpers/HitTestHelperTests.cs`
- Modify: `tests/NeatShot.Tests/Models/DrawingElementTests.cs`

**Interfaces:**
- Consumes: `DrawingToolType`, `DrawingElement`, `Rect`, `Point`
- Produces: 
  - `DrawingToolType.Ellipse`, `DrawingToolType.Line`, `DrawingToolType.Highlight`, `DrawingToolType.Text`, `DrawingToolType.Select`, `DrawingToolType.Eyedropper`
  - `DrawingElement.FontSize`, `DrawingElement.GetBoundingBox()`
  - `HitTestHelper.HitTest(DrawingElement element, Point testPoint, double tolerance = 6.0) -> bool`

- [x] **Step 1: Viet test that bai cho DrawingToolType moi va HitTestHelper**

```csharp
[Fact]
public void HitTestHelper_Rectangle_ReturnsTrue_WhenPointInsideOrOnBorder()
{
    var element = new DrawingElement
    {
        ToolType = DrawingToolType.Rectangle,
        Rect = new Rect(10, 10, 100, 50),
        Thickness = 3
    };

    Assert.True(HitTestHelper.HitTest(element, new Point(10, 10), 6.0));
    Assert.True(HitTestHelper.HitTest(element, new Point(50, 30), 6.0));
    Assert.False(HitTestHelper.HitTest(element, new Point(200, 200), 6.0));
}

[Fact]
public void HitTestHelper_LineAndEllipse_IdentifiesHitsCorrectly()
{
    var line = new DrawingElement
    {
        ToolType = DrawingToolType.Line,
        StartPoint = new Point(0, 0),
        EndPoint = new Point(100, 0),
        Thickness = 2
    };
    Assert.True(HitTestHelper.HitTest(line, new Point(50, 2), 6.0));
    Assert.False(HitTestHelper.HitTest(line, new Point(50, 20), 6.0));

    var ellipse = new DrawingElement
    {
        ToolType = DrawingToolType.Ellipse,
        Rect = new Rect(0, 0, 100, 100),
        Thickness = 2
    };
    Assert.True(HitTestHelper.HitTest(ellipse, new Point(50, 50), 6.0));
    Assert.False(HitTestHelper.HitTest(ellipse, new Point(120, 120), 6.0));
}
```

- [x] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "FullyQualifiedName~HitTestHelperTests"`
Expected: FAIL vi `DrawingToolType` chua co cac gia tri moi va `HitTestHelper` chua duoc dinh nghia.

- [x] **Step 3: Cap nhat `DrawingElement.cs` va tao `HitTestHelper.cs`**

1. Trong `src/NeatShot/Core/Models/DrawingElement.cs`:
   - Bo sung vao `DrawingToolType`: `Ellipse`, `Line`, `Highlight`, `Select`, `Eyedropper`.
   - Bo sung thuoc tinh `public double FontSize { get; set; } = 16.0;`
   - Bo sung phuong thuc `public Rect GetBoundingBox()` tinh toan hop bao quanh hinh dua tren loai cong cu.
2. Tao `src/NeatShot/Common/Helpers/HitTestHelper.cs`:
   - Trien khai `public static bool HitTest(DrawingElement element, Point testPoint, double tolerance = 6.0)`:
     - `Rectangle` / `Ellipse`: kiem tra khoang cach toi tam hoac nam trong `Rect.Inflate(tolerance, tolerance)`.
     - `Line`: tinh khoang cach tu diem toi doan thang `(StartPoint, EndPoint)`.
     - `Pencil` / `Highlight`: kiem tra khoang cach toi tung doan thang noi cac diem trong `Points`.
     - `Text`: kiem tra diem co nam trong hop bao cua text hay khong.

- [x] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~HitTestHelperTests"`
Expected: PASS (tat ca test hit-test deu xanh).

- [x] **Step 5: Commit**

```bash
git add src/NeatShot/Core/Models/DrawingElement.cs src/NeatShot/Common/Helpers/HitTestHelper.cs tests/NeatShot.Tests/Helpers/HitTestHelperTests.cs
git commit -m "feat: add extended tool types, font size, and HitTestHelper"
```

---

### Task 2: Mo rong Vector Drawing Renderer cho Ellipse, Line, Highlight va Text

**Files:**
- Modify: `src/NeatShot/Common/Helpers/DrawingRenderer.cs:28-54`
- Create: `tests/NeatShot.Tests/Helpers/DrawingRendererTests.cs`

**Interfaces:**
- Consumes: `DrawingRenderer.RenderElement(DrawingContext dc, DrawingElement element)`
- Produces: Rendering cho `DrawingToolType.Ellipse`, `DrawingToolType.Line`, `DrawingToolType.Highlight`, va `DrawingToolType.Text` tren `DrawingContext`

- [x] **Step 1: Viet test that bai cho DrawingRenderer voi cac kieu hinh moi**

```csharp
[Fact]
public void DrawingRenderer_RenderElement_DoesNotThrow_ForNewToolTypes()
{
    var visual = new DrawingVisual();
    using (var dc = visual.RenderOpen())
    {
        var ellipse = new DrawingElement { ToolType = DrawingToolType.Ellipse, Rect = new Rect(10, 10, 40, 30), Color = Colors.Red, Thickness = 2 };
        var line = new DrawingElement { ToolType = DrawingToolType.Line, StartPoint = new Point(0, 0), EndPoint = new Point(50, 50), Color = Colors.Blue, Thickness = 2 };
        var highlight = new DrawingElement { ToolType = DrawingToolType.Highlight, Points = new List<Point> { new(0, 0), new(30, 10) }, Color = Colors.Yellow, Thickness = 4 };
        var text = new DrawingElement { ToolType = DrawingToolType.Text, StartPoint = new Point(20, 20), Text = "Sample Note", Color = Colors.White, FontSize = 16 };

        DrawingRenderer.RenderElement(dc, ellipse);
        DrawingRenderer.RenderElement(dc, line);
        DrawingRenderer.RenderElement(dc, highlight);
        DrawingRenderer.RenderElement(dc, text);
    }
    Assert.NotNull(visual);
}
```

- [x] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "FullyQualifiedName~DrawingRendererTests"`
Expected: FAIL vi switch-case trong `DrawingRenderer` chua ho tro cac loai cong cu moi.

- [x] **Step 3: Trien khai rendering cho cac loai cong cu moi trong `DrawingRenderer.cs`**

1. Case `DrawingToolType.Ellipse`:
   - Tinh `center = new Point(element.Rect.X + element.Rect.Width / 2, element.Rect.Y + element.Rect.Height / 2)`.
   - `dc.DrawEllipse(null, pen, center, element.Rect.Width / 2, element.Rect.Height / 2)`.
2. Case `DrawingToolType.Line`:
   - `dc.DrawLine(pen, element.StartPoint, element.EndPoint)`.
3. Case `DrawingToolType.Highlight`:
   - Tao but semi-transparent: mau co kenh Alpha = 120 (`Color.FromArgb(120, element.Color.R, element.Color.G, element.Color.B)`).
   - Do day net but da quang day hon: `Math.Max(16.0, element.Thickness * 3.5)`, dau but phang `PenLineCap.Square`.
   - Ve duong polyline bang `StreamGeometry` qua danh sach `element.Points`.
4. Case `DrawingToolType.Text`:
   - Neu `string.IsNullOrWhiteSpace(element.Text)` thi bo qua.
   - Khoi tao `FormattedText(element.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), element.FontSize > 0 ? element.FontSize : 16.0, brush, 1.0)`.
   - `dc.DrawText(formattedText, element.StartPoint)`.

- [x] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~DrawingRendererTests"`
Expected: PASS (tat ca tool types deu duoc render thanh cong).

- [x] **Step 5: Commit**

```bash
git add src/NeatShot/Common/Helpers/DrawingRenderer.cs tests/NeatShot.Tests/Helpers/DrawingRendererTests.cs
git commit -m "feat: implement rendering for ellipse, line, highlight, and text"
```

---

### Task 3: Tuong tac ve hinh Oval, Line va Highlight tren DrawingCanvas

**Files:**
- Modify: `src/NeatShot/Presentation/Controls/DrawingCanvas.cs:88-95,170-198`
- Modify: `tests/NeatShot.Tests/Controls/DrawingCanvasTests.cs`

**Interfaces:**
- Consumes: `DrawingToolType.Ellipse`, `DrawingToolType.Line`, `DrawingToolType.Highlight`
- Produces: `DrawingCanvas` xu ly `OnMouseMove` va `OnMouseLeftButtonUp` de tao doi tuong `DrawingElement` phu hop va cap nhat visual

- [ ] **Step 1: Viet test that bai cho viec switch tool va mouse movement tren DrawingCanvas**

```csharp
[Fact]
public void DrawingCanvas_SupportsEllipseLineAndHighlight_ToolSwitching_OnStaThread()
{
    Exception? threadException = null;
    var thread = new Thread(() =>
    {
        try
        {
            var canvas = new DrawingCanvas();
            canvas.CurrentTool = DrawingToolType.Ellipse;
            Assert.True(canvas.IsHitTestVisible);
            Assert.Equal(Cursors.Cross, canvas.Cursor);

            canvas.CurrentTool = DrawingToolType.Line;
            Assert.True(canvas.IsHitTestVisible);
            Assert.Equal(Cursors.Cross, canvas.Cursor);

            canvas.CurrentTool = DrawingToolType.Highlight;
            Assert.True(canvas.IsHitTestVisible);
            Assert.Equal(Cursors.Pen, canvas.Cursor);
        }
        catch (Exception ex)
        {
            threadException = ex;
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    Assert.Null(threadException);
}
```

- [ ] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "DrawingCanvas_SupportsEllipseLineAndHighlight"`
Expected: FAIL vi cursor va trang thai cua `DrawingCanvas` chua xu ly cac case nay.

- [ ] **Step 3: Cap nhat `DrawingCanvas.cs` ho tro Ellipse, Line, Highlight**

1. Trong `UpdateToolState`:
   - Cursor switch:
     - `DrawingToolType.Pencil` hoac `DrawingToolType.Highlight` => `Cursors.Pen`
     - `DrawingToolType.Rectangle`, `DrawingToolType.Ellipse`, `DrawingToolType.Line`, `DrawingToolType.Arrow` => `Cursors.Cross`
2. Trong `OnMouseMove`:
   - Case `DrawingToolType.Highlight`:
     - Them toa do hien tai vao `_currentElement.Points.Add(currentPoint)`.
   - Case `DrawingToolType.Line`:
     - Cap nhat `_currentElement.EndPoint = currentPoint`.
   - Case `DrawingToolType.Ellipse`:
     - Tinh toan hop chu nhat bao quanh tu `_currentElement.StartPoint` den `currentPoint`:
       `minX = Math.Min(_currentElement.StartPoint.X, currentPoint.X);`
       `minY = Math.Min(_currentElement.StartPoint.Y, currentPoint.Y);`
       `width = Math.Abs(currentPoint.X - _currentElement.StartPoint.X);`
       `height = Math.Abs(currentPoint.Y - _currentElement.StartPoint.Y);`
       `_currentElement.Rect = new Rect(minX, minY, width, height);`
       `_currentElement.EndPoint = currentPoint;`

- [ ] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~DrawingCanvasTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/NeatShot/Presentation/Controls/DrawingCanvas.cs tests/NeatShot.Tests/Controls/DrawingCanvasTests.cs
git commit -m "feat: add ellipse, line, and highlight drawing interaction to DrawingCanvas"
```

---

### Task 4: Cong cu chen Text truc tiep tren DrawingCanvas

**Files:**
- Modify: `src/NeatShot/Presentation/Controls/DrawingCanvas.cs`
- Create: `tests/NeatShot.Tests/Controls/DrawingCanvasTextTests.cs`

**Interfaces:**
- Consumes: `DrawingToolType.Text`, `CurrentColor`, `CurrentThickness`
- Produces: 
  - Hien thi inline `TextBox` khi click vao canvas o che do Text
  - Commit text vao `DrawingElement` (ToolType = Text) va day vao `UndoStack` khi Enter / LostFocus
  - Huy bo khi nhan Escape hoac de trong

- [ ] **Step 1: Viet test that bai cho logic Text Annotation**

```csharp
[Fact]
public void CommitTextAnnotation_AddsTextElementToUndoStack_OnStaThread()
{
    Exception? threadException = null;
    var thread = new Thread(() =>
    {
        try
        {
            var canvas = new DrawingCanvas();
            canvas.CurrentTool = DrawingToolType.Text;
            canvas.CurrentColor = Colors.White;

            canvas.CommitText("Ghi chu test", new Point(100, 150));

            Assert.Single(canvas.UndoStack.Items);
            var item = canvas.UndoStack.Items.First();
            Assert.Equal(DrawingToolType.Text, item.ToolType);
            Assert.Equal("Ghi chu test", item.Text);
            Assert.Equal(100, item.StartPoint.X);
            Assert.Equal(150, item.StartPoint.Y);
        }
        catch (Exception ex)
        {
            threadException = ex;
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    Assert.Null(threadException);
}
```

- [ ] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "CommitTextAnnotation_AddsTextElementToUndoStack"`
Expected: FAIL vi phuong thuc `CommitText` chua ton tai.

- [ ] **Step 3: Trien khai inline TextBox va phuong thuc `CommitText` trong `DrawingCanvas.cs`**

1. Khai bao `private TextBox? _inlineEditor;` va `private Point _textPosition;`.
2. Bo sung phuong thuc public de testable: `public void CommitText(string text, Point position)`:
   - Neu text khong rong: tao `DrawingElement` moi voi `ToolType = DrawingToolType.Text`, `Text = text`, `StartPoint = position`, `Color = CurrentColor`, `FontSize = 16.0`.
   - Day vao `UndoStack.Push(element)`.
   - `InvalidateVisual()`.
3. Trong `OnMouseLeftButtonDown`:
   - Neu `CurrentTool == DrawingToolType.Text`:
     - Neu dang co `_inlineEditor`, goi ham commit hoac remove.
     - Tao moi `_inlineEditor = new TextBox()`:
       - Background trong suot hoac nen toi mờ (`#80000000`), chu trang/theo mau `CurrentColor`, vien dut net mong.
       - Font size 15pt, `AcceptsReturn = false`.
       - Gan vi tri `Canvas.SetLeft(_inlineEditor, pos.X)`, `Canvas.SetTop(_inlineEditor, pos.Y)`.
       - Dang ky su kien `KeyDown`: neu nhan `Key.Enter` goi commit va go bo editor; neu nhan `Key.Escape` chi go bo editor.
       - Dang ky su kien `LostFocus`: goi commit va go bo editor.
     - Them vao `Children.Add(_inlineEditor)`.
     - `_inlineEditor.Focus()`.
     - `e.Handled = true;`.

- [ ] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~DrawingCanvasTextTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/NeatShot/Presentation/Controls/DrawingCanvas.cs tests/NeatShot.Tests/Controls/DrawingCanvasTextTests.cs
git commit -m "feat: implement inline text annotation tool in DrawingCanvas"
```

---

### Task 5: Cong cu Select & Move hinh ve da tao

**Files:**
- Modify: `src/NeatShot/Presentation/Controls/DrawingCanvas.cs`
- Create: `tests/NeatShot.Tests/Controls/DrawingCanvasSelectMoveTests.cs`

**Interfaces:**
- Consumes: `HitTestHelper.HitTest`, `DrawingToolType.Select`
- Produces: 
  - `DrawingCanvas.SelectedElement` (hien thi khung vien bao quanh doi tuong duoc chon)
  - Keo chuot di chuyen rieng le doi tuong duoc chon (`TranslateElement`)
  - Ho tro xoa phan tu duoc chon khi nhan phan tu Delete

- [ ] **Step 1: Viet test that bai cho Select va Move doi tuong ve**

```csharp
[Fact]
public void SelectAndMove_TranslatesSpecificElement_OnStaThread()
{
    Exception? threadException = null;
    var thread = new Thread(() =>
    {
        try
        {
            var canvas = new DrawingCanvas();
            var rect = new DrawingElement
            {
                ToolType = DrawingToolType.Rectangle,
                Rect = new Rect(20, 20, 50, 50),
                StartPoint = new Point(20, 20),
                EndPoint = new Point(70, 70)
            };
            canvas.UndoStack.Push(rect);

            // Act: Select element tai vi tri (25, 25)
            canvas.SelectElementAt(new Point(25, 25));
            Assert.NotNull(canvas.SelectedElement);
            Assert.Same(rect, canvas.SelectedElement);

            // Move doi tuong di chuyen dx = 10, dy = 15
            canvas.MoveSelectedElement(10, 15);
            Assert.Equal(30, canvas.SelectedElement.Rect.X);
            Assert.Equal(35, canvas.SelectedElement.Rect.Y);
        }
        catch (Exception ex)
        {
            threadException = ex;
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    Assert.Null(threadException);
}
```

- [ ] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "SelectAndMove_TranslatesSpecificElement"`
Expected: FAIL vi `SelectElementAt` va `MoveSelectedElement` chua duoc cai dat.

- [ ] **Step 3: Trien khai Select va Move trong `DrawingCanvas.cs`**

1. Khai bao thuoc tinh `public DrawingElement? SelectedElement { get; private set; }`.
2. Trien khai `public bool SelectElementAt(Point pos)`:
   - Duyet nguoc tu cuoi danh sach `UndoStack.Items` ve dau (phan tu ve sau nam tren cung).
   - Su dung `HitTestHelper.HitTest(item, pos)`. Neu trung, gan `SelectedElement = item` va goi `InvalidateVisual()`, tra ve true.
   - Neu khong trung phan tu nao, gan `SelectedElement = null`, goi `InvalidateVisual()`, tra ve false.
3. Trien khai `public void MoveSelectedElement(double dx, double dy)`:
   - Neu `SelectedElement == null` thi return.
   - Di chuyen `StartPoint` va `EndPoint` theo `dx, dy`.
   - Neu co `Rect`, cap nhat toa do `Rect.X += dx`, `Rect.Y += dy`.
   - Neu co danh sach `Points`, cong `dx, dy` cho tat ca cac diem.
   - Goi `InvalidateVisual()`.
4. Trong `OnMouseMove`:
   - Khi `CurrentTool == DrawingToolType.Select`:
     - Neu dang giu chuot va co `SelectedElement`, tinh toan `dx = pos.X - _lastPoint.X`, `dy = pos.Y - _lastPoint.Y`, goi `MoveSelectedElement(dx, dy)`.
     - Cap nhat cursor: `Cursors.SizeAll` khi hover qua phan tu hoac khi dang keo di chuyen.
5. Trong `OnRender`:
   - Neu co `SelectedElement`, ve khung vien cham dut (dashed bounding box) mau xanh duong nhat bao quanh hop gioi han cua doi tuong de nguoi dung nhan biet doi tuong dang duoc chon.

- [ ] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~DrawingCanvasSelectMoveTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/NeatShot/Presentation/Controls/DrawingCanvas.cs tests/NeatShot.Tests/Controls/DrawingCanvasSelectMoveTests.cs
git commit -m "feat: implement select and move individual annotations on DrawingCanvas"
```

---

### Task 6: Tinh nang Cay but hut mau tren man hinh (Eyedropper / Screen Color Picker)

**Files:**
- Create: `src/NeatShot/Common/Helpers/ColorPickerHelper.cs`
- Create: `tests/NeatShot.Tests/Helpers/ColorPickerHelperTests.cs`
- Modify: `src/NeatShot/Presentation/Controls/DrawingCanvas.cs`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml`
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`

**Interfaces:**
- Consumes: `BitmapSource`, `Point` (DIP), DpiX, DpiY
- Produces: 
  - `ColorPickerHelper.GetPixelColor(BitmapSource bitmap, Point dipPoint, double dpiX = 96.0, double dpiY = 96.0) -> Color`
  - Che do Eyedropper tren Overlay: kinh lup (Loupe badge) hien thi pixel mau phong to va ma HEX `#RRGGBB` tai con tro chuot.
  - Khi click: lay mau, kich hoat su kien `ColorPicked(Color)`, them vao lich su mau va tro lai cong cu ve truoc do.

- [ ] **Step 1: Viet test that bai cho ColorPickerHelper**

```csharp
[Fact]
public void ColorPickerHelper_ExtractsAccuratePixelColor_FromBitmapSource()
{
    // Tao mot WriteableBitmap 2x2 voi cac pixel mau sac cu the
    var bitmap = new WriteableBitmap(2, 2, 96, 96, PixelFormats.Bgra32, null);
    var pixels = new byte[]
    {
        0x00, 0x00, 0xFF, 0xFF, // Red (BGRA: B=0, G=0, R=255, A=255)
        0x00, 0xFF, 0x00, 0xFF, // Green
        0xFF, 0x00, 0x00, 0xFF, // Blue
        0xFF, 0xFF, 0xFF, 0xFF  // White
    };
    bitmap.WritePixels(new Int32Rect(0, 0, 2, 2), pixels, 8, 0);

    var redColor = ColorPickerHelper.GetPixelColor(bitmap, new Point(0, 0), 96, 96);
    var greenColor = ColorPickerHelper.GetPixelColor(bitmap, new Point(1, 0), 96, 96);
    var blueColor = ColorPickerHelper.GetPixelColor(bitmap, new Point(0, 1), 96, 96);

    Assert.Equal(Color.FromRgb(255, 0, 0), redColor);
    Assert.Equal(Color.FromRgb(0, 255, 0), greenColor);
    Assert.Equal(Color.FromRgb(0, 0, 255), blueColor);
}
```

- [ ] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "FullyQualifiedName~ColorPickerHelperTests"`
Expected: FAIL vi `ColorPickerHelper` chua ton tai.

- [ ] **Step 3: Trien khai `ColorPickerHelper.cs`**

1. Tao `src/NeatShot/Common/Helpers/ColorPickerHelper.cs`:
   - `public static Color GetPixelColor(BitmapSource bitmap, Point dipPoint, double dpiX = 96.0, double dpiY = 96.0)`:
     - Tinh ti le scale: `scaleX = dpiX / 96.0; scaleY = dpiY / 96.0;`
     - Tinh toa do pixel vat ly:
       `px = (int)Math.Clamp(Math.Round(dipPoint.X * scaleX), 0, bitmap.PixelWidth - 1);`
       `py = (int)Math.Clamp(Math.Round(dipPoint.Y * scaleY), 0, bitmap.PixelHeight - 1);`
     - Chuyen doi format ve `Bgra32` hoac doc truc tiep qua `CopyPixels(new Int32Rect(px, py, 1, 1), buffer, 4, 0)`.
     - Tra ve `Color.FromRgb(buffer[2], buffer[1], buffer[0])`.

- [ ] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~ColorPickerHelperTests"`
Expected: PASS.

- [ ] **Step 5: Tich hop Eyedropper UI vao `OverlayWindow`**

1. Trong `OverlayWindow.xaml`:
   - Them `Border x:Name="EyedropperLoupe"` (kinh lup badge mien phi di dong theo chuot):
     - Gom hinh tron mau hien tai, text ma mau HEX `#RRGGBB`, va vien bong tron.
     - Mac dinh `Visibility="Collapsed"`.
2. Trong `OverlayWindow.xaml.cs`:
   - Khi `Toolbar` chon Eyedropper:
     - Bat `EyedropperLoupe.Visibility = Visibility.Visible`.
     - Khi chuot di chuyen tren window: cap nhat toa do kinh lup cach con tro 15px, doc ma mau tai toa do chuot bang `ColorPickerHelper`, hien thi mau va chuoi HEX.
     - Khi chuot click: lay mau do, cap nhat vao `Toolbar.SetCustomColor(pickedColor)`, an `EyedropperLoupe`, va kich hoat lai cong cu ve truoc do.

- [ ] **Step 6: Commit**

```bash
git add src/NeatShot/Common/Helpers/ColorPickerHelper.cs tests/NeatShot.Tests/Helpers/ColorPickerHelperTests.cs src/NeatShot/Presentation/Views/OverlayWindow.xaml src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs
git commit -m "feat: add eyedropper screen color picker with loupe preview"
```

---

### Task 7: Khung luu Lich su mau (Color History Palette) va Custom Color

**Files:**
- Create: `src/NeatShot/Core/Models/ColorHistory.cs`
- Create: `tests/NeatShot.Tests/Models/ColorHistoryTests.cs`
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml`
- Modify: `src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml.cs`
- Modify: `tests/NeatShot.Tests/Controls/AnnotationToolbarTests.cs`

**Interfaces:**
- Consumes: `ColorHistory`, `Color`
- Produces: 
  - `ColorHistory.AddColor(Color)` (LRU queue toi da 8 mau, loai tru trung lap)
  - `AnnotationToolbar` hien thi hang o mau lich su (Color History Swatches)
  - Nut Eyedropper (`💉`) va nut chon mau tuy chinh (Custom Color)
  - Chon mau tu lich su phat ra su kien `ColorSelected` nhu mau mac dinh

- [ ] **Step 1: Viet test that bai cho ColorHistory**

```csharp
[Fact]
public void ColorHistory_AddsAndDeduplicatesColors_UpToCapacity()
{
    var history = new ColorHistory(capacity: 3);

    history.AddColor(Colors.Red);
    history.AddColor(Colors.Green);
    history.AddColor(Colors.Blue);

    Assert.Equal(3, history.Colors.Count);
    Assert.Equal(Colors.Blue, history.Colors[0]);

    // Them lai Red: Red phai duoc day len dau danh sach, khong bi duplicate
    history.AddColor(Colors.Red);
    Assert.Equal(3, history.Colors.Count);
    Assert.Equal(Colors.Red, history.Colors[0]);

    // Them mau moi vuot qua dung luong 3: mau cu nhat (Green) bi day ra ngoai
    history.AddColor(Colors.Yellow);
    Assert.Equal(3, history.Colors.Count);
    Assert.Equal(Colors.Yellow, history.Colors[0]);
    Assert.DoesNotContain(Colors.Green, history.Colors);
}
```

- [ ] **Step 2: Chay test de xac nhan test that bai**

Run: `dotnet test --filter "FullyQualifiedName~ColorHistoryTests"`
Expected: FAIL vi `ColorHistory` chua duoc tao.

- [ ] **Step 3: Trien khai `ColorHistory.cs`**

1. Tao `src/NeatShot/Core/Models/ColorHistory.cs`:
   - `public class ColorHistory`
   - `public ObservableCollection<Color> Colors { get; } = new();`
   - `public int Capacity { get; } = 8;`
   - `public void AddColor(Color color)`:
     - Neu mau da ton tai trong danh sach (so sanh R, G, B, A), go bo vi tri cu.
     - Chen vao vi tri index 0 (`Colors.Insert(0, color)`).
     - Neu `Colors.Count > Capacity`, loai bo phan tu cuoi cung.

- [ ] **Step 4: Chay test de xac nhan test vuot qua**

Run: `dotnet test --filter "FullyQualifiedName~ColorHistoryTests"`
Expected: PASS.

- [ ] **Step 5: Cap nhat giao dien `AnnotationToolbar.xaml` va code-behind**

1. Trong `AnnotationToolbar.xaml`:
   - Them nut Oval (`EllipseButton` voi ky hieu `⬭`).
   - Them nut Duong thang (`LineButton` voi ky hieu `―`).
   - Them nut Da quang (`HighlightButton` voi ky hieu `🖍️` hoac `HIGHLIGHT`).
   - Them nut Text (`TextButton` voi ky hieu `T`).
   - Them nut Chon/Di chuyen (`SelectButton` voi ky hieu `👆`).
   - Them nut But hut mau (`EyedropperButton` voi ky hieu `💉`).
   - Them khung chua danh sach lich su mau: `ItemsControl x:Name="HistoryColorsPanel"` hien thi cac o tron mau swatch vua duoc them.
2. Trong `AnnotationToolbar.xaml.cs`:
   - Quan ly `ColorHistory History { get; } = new(capacity: 6);`.
   - Phuong thuc `public void AddColorToHistory(Color color)`: goi `History.AddColor(color)` va cap nhat lai cac o swatch.
   - Click vao bat ky o mau lich su nao cung phat ra su kien `ColorSelected` va highlight vien trang.

- [ ] **Step 6: Viet test va xac nhan test tren STA thread**

Run: `dotnet test --filter "FullyQualifiedName~AnnotationToolbarTests"`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/NeatShot/Core/Models/ColorHistory.cs tests/NeatShot.Tests/Models/ColorHistoryTests.cs src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml src/NeatShot/Presentation/Controls/AnnotationToolbar.xaml.cs tests/NeatShot.Tests/Controls/AnnotationToolbarTests.cs
git commit -m "feat: add color history palette and new toolbar tool buttons"
```

---

### Task 8: Tich hop toan dien vao OverlayWindow va Kiem thu Export

**Files:**
- Modify: `src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs`
- Create: `tests/NeatShot.Tests/Services/ExportServiceAdvancedTests.cs`

**Interfaces:**
- Consumes: Tat ca cac cong cu moi (Ellipse, Line, Highlight, Text, Move, Eyedropper, History)
- Produces: Luong nguoi dung lien mach tren OverlayWindow va xuat anh hoan chinh ra Clipboard / PNG qua `ExportService`.

- [ ] **Step 1: Viet test that bai cho ExportService voi cac hinh ve va text moi**

```csharp
[Fact]
public void ExportService_RendersAllNewAnnotationTypes_Correctly()
{
    var screenServiceMock = new MockScreenCaptureService();
    var exportService = new ExportService(screenServiceMock);

    var background = new WriteableBitmap(400, 300, 96, 96, PixelFormats.Bgra32, null);
    var region = new CaptureRegion(50, 50, 200, 150);

    var annotations = new List<DrawingElement>
    {
        new() { ToolType = DrawingToolType.Ellipse, Rect = new Rect(60, 60, 40, 30), Color = Colors.Red, Thickness = 2 },
        new() { ToolType = DrawingToolType.Line, StartPoint = new Point(70, 70), EndPoint = new Point(120, 120), Color = Colors.Green, Thickness = 2 },
        new() { ToolType = DrawingToolType.Highlight, Points = new List<Point> { new(80, 80), new(150, 80) }, Color = Colors.Yellow, Thickness = 16 },
        new() { ToolType = DrawingToolType.Text, StartPoint = new Point(90, 90), Text = "Test Note", Color = Colors.White, FontSize = 16 }
    };

    var finalImage = exportService.RenderFinalImage(background, region, annotations);

    Assert.NotNull(finalImage);
    Assert.Equal(200, finalImage.PixelWidth);
    Assert.Equal(150, finalImage.PixelHeight);
}
```

- [ ] **Step 2: Chay test de xac nhan ket qua**

Run: `dotnet test --filter "ExportService_RendersAllNewAnnotationTypes_Correctly"`
Expected: PASS (do `DrawingRenderer` o Task 2 da duoc chia se va ke thua boi `ExportService`).

- [ ] **Step 3: Ket noi su kien giua `OverlayWindow`, `Toolbar`, va `DrawingControl`**

1. Trong `OverlayWindow.xaml.cs`:
   - Ket noi cac nut cong cu moi tu Toolbar vao `DrawingControl.CurrentTool`.
   - Ket noi nut Eyedropper: bat che do hut mau tren Overlay, khi click man hinh se goi `ColorPickerHelper`, day mau vao `Toolbar.AddColorToHistory` va gan lam `DrawingControl.CurrentColor`.
   - Phim tat bo sung:
     - Phim `T`: kich hoat Text tool.
     - Phim `H`: kich hoat Highlight tool.
     - Phim `V` hoac `S`: kich hoat Select tool.
     - Phim `I` hoac `C`: kich hoat Eyedropper tool.
     - Phim `Delete`: xoa phan tu hien dang duoc chon trong `DrawingControl.SelectedElement`.

- [ ] **Step 4: Chay toan bo bo test xUnit**

Run: `dotnet test`
Expected: PASS tat ca cac test (>= 75 tests), khong co test nao fail hay skip.

- [ ] **Step 5: Commit**

```bash
git add src/NeatShot/Presentation/Views/OverlayWindow.xaml.cs tests/NeatShot.Tests/Services/ExportServiceAdvancedTests.cs
git commit -m "feat: integrate advanced annotations, eyedropper, and color history in OverlayWindow"
```

---

## Plan Self-Review Checklist

1. **Spec coverage:** 
   - Ve oval: Task 1, 2, 3, 7
   - Ve duong thang: Task 1, 2, 3, 7
   - Di chuyen cac hinh da ve: Task 1, 5, 8
   - Tinh nang highlight: Task 1, 2, 3, 7
   - Nhap text vao khung chu: Task 1, 2, 4, 7
   - Cay but hut mau he thong: Task 1, 6, 8
   - Khung luu lich su mau chon: Task 7, 8
2. **Step scan:** Moi buoc deu co test ro rang, lenh kiem tra `dotnet test`, va git commit rieng biet.
3. **Type consistency:** Cac ten enum `DrawingToolType.Ellipse`, `Line`, `Highlight`, `Text`, `Select`, `Eyedropper` nhat quan xuyen suot tu Task 1 den Task 8.
4. **Review Focus:** 5 truong hop rui ro/ngoai le deu duoc kiem tra trong cac test tuong ung.
5. **Proportion:** Ke hoach suc tich, ro rang ve interface va hanh vi, khong viet lai toan bo code ung dung.
