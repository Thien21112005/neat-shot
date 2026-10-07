using System.Runtime.InteropServices;

namespace NeatShot.Core.Interop;

/// <summary>
/// Win32 RECT structure đại diện cho toạ độ hình chữ nhật pixel.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public int Width => Right - Left;
    public int Height => Bottom - Top;
}

/// <summary>
/// Win32 POINT structure đại diện cho toạ độ điểm (X, Y).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}
