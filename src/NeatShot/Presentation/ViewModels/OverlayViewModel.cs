using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeatShot.Core.Models;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.ViewModels;

/// <summary>
/// ViewModel quản lý trạng thái của cửa sổ Overlay toàn màn hình (ảnh nền đóng băng, vùng chọn).
/// </summary>
public partial class OverlayViewModel : ObservableObject
{
    [ObservableProperty]
    private BitmapSource? _backgroundImage;

    [ObservableProperty]
    private CaptureRegion _selectedRegion;

    [ObservableProperty]
    private bool _hasSelection;

    [ObservableProperty]
    private bool _isSelecting;

    public event EventHandler? RequestClose;

    partial void OnSelectedRegionChanged(CaptureRegion value)
    {
        HasSelection = value.IsValid;
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedRegion = default;
        HasSelection = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
