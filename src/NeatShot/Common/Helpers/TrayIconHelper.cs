using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Hỗ trợ tạo biểu tượng icon cho khay hệ thống (System Tray) bằng vector WPF sắc nét.
/// </summary>
public static class TrayIconHelper
{
    public static ImageSource CreateTrayIcon()
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            // Nền xanh dương hiện đại bo góc (#0078D4)
            var bgBrush = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            bgBrush.Freeze();
            dc.DrawRoundedRectangle(bgBrush, null, new Rect(0, 0, 32, 32), 6, 6);

            // Viền máy ảnh / khung chụp màu trắng
            var pen = new Pen(Brushes.White, 2.2);
            pen.Freeze();
            dc.DrawRoundedRectangle(null, pen, new Rect(5, 9, 22, 17), 3, 3);
            dc.DrawEllipse(null, pen, new Point(16, 17.5), 4.5, 4.5);

            // Nút bấm trên máy ảnh
            dc.DrawRoundedRectangle(Brushes.White, null, new Rect(8, 6, 6, 3), 1, 1);
        }

        var rtb = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(visual);
        rtb.Freeze();
        return rtb;
    }
}
