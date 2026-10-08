using NeatShot.Core.Interop;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Hỗ trợ tạo biểu tượng icon cho khay hệ thống (System Tray) dưới dạng System.Drawing.Icon tương thích hoàn toàn với Windows Shell.
/// </summary>
public static class TrayIconHelper
{
    public static Icon CreateTrayIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Nền bo góc xanh dương công nghệ (#0078D4)
            using var bgBrush = new SolidBrush(Color.FromArgb(0, 120, 212));
            using var path = CreateRoundedRectanglePath(new Rectangle(0, 0, 31, 31), 6);
            g.FillPath(bgBrush, path);

            // Khung máy ảnh màu trắng
            using var whitePen = new Pen(Color.White, 2);
            using var bodyPath = CreateRoundedRectanglePath(new Rectangle(5, 9, 21, 15), 3);
            g.DrawPath(whitePen, bodyPath);

            // Ống kính máy ảnh
            g.DrawEllipse(whitePen, 11, 12, 9, 9);

            // Nút bấm trên đỉnh máy ảnh
            using var whiteBrush = new SolidBrush(Color.White);
            g.FillRectangle(whiteBrush, 8, 6, 6, 3);
        }

        var hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

        // Góc trên bên trái
        path.AddArc(arc, 180, 90);

        // Góc trên bên phải
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);

        // Góc dưới bên phải
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // Góc dưới bên trái
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }
}
