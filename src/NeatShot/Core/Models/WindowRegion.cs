using System.Windows;

namespace NeatShot.Core.Models;

/// <summary>
/// Vùng của một cửa sổ (hoặc control con) trên màn hình, dùng để tự nhận diện khi rê chuột ở màn hình chụp.
/// </summary>
/// <param name="Bounds">Hình chữ nhật của cửa sổ.</param>
/// <param name="TopLevelIndex">Thứ tự z-order của cửa sổ cấp cao nhất chứa vùng này (0 = trên cùng).</param>
/// <param name="IsChild">true nếu là control/panel con bên trong cửa sổ.</param>
public readonly record struct WindowRegion(Rect Bounds, int TopLevelIndex, bool IsChild);
