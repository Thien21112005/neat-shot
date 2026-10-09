using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.ViewModels;

/// <summary>
/// ViewModel quản lý thư viện ảnh chụp màn hình NeatShot.
/// </summary>
public partial class GalleryViewModel : ObservableObject
{
    private readonly IScreenshotGalleryService _galleryService;

    public ObservableCollection<GalleryItem> Items { get; } = new();

    [ObservableProperty]
    private GalleryItem? _selectedItem;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _screenshotsDirectory = string.Empty;

    public bool HasItems => Items.Count > 0;
    public bool IsEmpty => Items.Count == 0;

    public event EventHandler<BitmapSource>? PinRequested;

    public Action<string>? FolderOpenerAction { get; set; }

    public GalleryViewModel(IScreenshotGalleryService galleryService)
    {
        _galleryService = galleryService ?? throw new ArgumentNullException(nameof(galleryService));
        _screenshotsDirectory = _galleryService.GetScreenshotsDirectory();
    }

    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        IsLoading = true;
        try
        {
            ScreenshotsDirectory = _galleryService.GetScreenshotsDirectory();
            var list = await _galleryService.GetSavedScreenshotsAsync();

            Items.Clear();
            foreach (var item in list)
            {
                Items.Add(item);
            }

            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(IsEmpty));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void PinItem(GalleryItem? item)
    {
        if (item == null) return;

        var bitmap = _galleryService.LoadImage(item.FilePath);
        if (bitmap != null)
        {
            PinRequested?.Invoke(this, bitmap);
        }
    }

    [RelayCommand]
    public void CopyItem(GalleryItem? item)
    {
        if (item == null) return;

        var bitmap = _galleryService.LoadImage(item.FilePath);
        if (bitmap != null)
        {
            try
            {
                System.Windows.Clipboard.SetImage(bitmap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GalleryViewModel] Lỗi sao chép ảnh: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public async Task<bool> DeleteItemAsync(GalleryItem? item)
    {
        if (item == null) return false;

        var deleted = _galleryService.DeleteScreenshot(item.FilePath);
        if (deleted)
        {
            Items.Remove(item);
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(IsEmpty));
            return true;
        }

        return false;
    }

    [RelayCommand]
    public void OpenFolder()
    {
        var dir = _galleryService.GetScreenshotsDirectory();
        if (FolderOpenerAction != null)
        {
            FolderOpenerAction(dir);
            return;
        }

        if (Directory.Exists(dir))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GalleryViewModel] Không thể mở thư mục: {ex.Message}");
            }
        }
    }
}
