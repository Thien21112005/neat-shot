using Microsoft.Win32.SafeHandles;
using NeatShot.Core.Interop;

namespace NeatShot.Core.Interop;

/// <summary>
/// Quản lý vòng đời của Icon/Cursor handle (HICON/HCURSOR) an toàn, tự động gọi DestroyIcon khi dispose.
/// </summary>
public sealed class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeIconHandle() : base(true)
    {
    }

    public SafeIconHandle(IntPtr handle) : base(true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        return NativeMethods.DestroyIcon(handle);
    }
}
