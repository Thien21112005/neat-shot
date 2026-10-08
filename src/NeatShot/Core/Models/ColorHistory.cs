using System.Collections.ObjectModel;
using System.Windows.Media;

namespace NeatShot.Core.Models;

/// <summary>
/// Quản lý danh sách lịch sử màu sắc người dùng đã chọn hoặc hút bằng công cụ Eyedropper.
/// Áp dụng cơ chế LRU (Least Recently Used), tự động loại trừ trùng lặp và giới hạn dung lượng tối đa.
/// </summary>
public class ColorHistory
{
    /// <summary>
    /// Danh sách màu được lưu, màu mới nhất luôn nằm ở đầu (index 0).
    /// </summary>
    public ObservableCollection<Color> Colors { get; } = new();

    /// <summary>
    /// Số lượng màu tối đa có thể lưu trữ trong lịch sử.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Khởi tạo đối tượng lưu trữ lịch sử màu sắc với dung lượng quy định (mặc định 8 màu).
    /// </summary>
    /// <param name="capacity">Số lượng màu tối đa.</param>
    public ColorHistory(int capacity = 8)
    {
        Capacity = capacity > 0 ? capacity : 8;
    }

    /// <summary>
    /// Thêm một màu vào danh sách lịch sử. Nếu màu đã tồn tại, đưa màu đó lên đầu.
    /// Nếu danh sách vượt quá dung lượng, loại bỏ màu cũ nhất ở cuối.
    /// </summary>
    /// <param name="color">Màu cần thêm.</param>
    public void AddColor(Color color)
    {
        // Kiểm tra xem màu đã có trong danh sách chưa
        for (int i = 0; i < Colors.Count; i++)
        {
            var c = Colors[i];
            if (c.A == color.A && c.R == color.R && c.G == color.G && c.B == color.B)
            {
                Colors.RemoveAt(i);
                break;
            }
        }

        // Đưa màu mới nhất lên vị trí đầu tiên
        Colors.Insert(0, color);

        // Loại bỏ phần tử vượt quá dung lượng
        while (Colors.Count > Capacity)
        {
            Colors.RemoveAt(Colors.Count - 1);
        }
    }

    /// <summary>
    /// Xoá toàn bộ lịch sử màu.
    /// </summary>
    public void Clear()
    {
        Colors.Clear();
    }
}
