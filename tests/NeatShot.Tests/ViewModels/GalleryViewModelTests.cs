using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.ViewModels;

public class GalleryViewModelTests
{
    private class FakeScreenshotGalleryService : IScreenshotGalleryService
    {
        public List<GalleryItem> StoredItems { get; set; } = new();
        public string FakeDirectory { get; set; } = @"C:\FakePictures\NeatShot";
        public BitmapSource? FakeLoadedBitmap { get; set; }
        public bool DeleteResult { get; set; } = true;

        public string GetScreenshotsDirectory() => FakeDirectory;

        public Task<IReadOnlyList<GalleryItem>> GetSavedScreenshotsAsync()
        {
            return Task.FromResult<IReadOnlyList<GalleryItem>>(StoredItems.ToList());
        }

        public Task<string> SaveScreenshotAsync(BitmapSource image, string? filename = null)
        {
            return Task.FromResult(Path.Combine(FakeDirectory, filename ?? "shot.png"));
        }

        public bool DeleteScreenshot(string filePath)
        {
            var item = StoredItems.FirstOrDefault(x => x.FilePath == filePath);
            if (item != null)
            {
                StoredItems.Remove(item);
            }
            return DeleteResult;
        }

        public BitmapSource? LoadImage(string filePath)
        {
            return FakeLoadedBitmap;
        }
    }

    private static RenderTargetBitmap CreateDummyBitmap(int width = 100, int height = 100)
    {
        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Freeze();
        return rtb;
    }

    [Fact]
    public void GalleryViewModel_InitialState_IsEmpty()
    {
        var fakeService = new FakeScreenshotGalleryService();
        var vm = new GalleryViewModel(fakeService);

        Assert.NotNull(vm.Items);
        Assert.Empty(vm.Items);
        Assert.False(vm.HasItems);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task LoadItemsAsync_PopulatesItems_AndUpdatesState()
    {
        var fakeService = new FakeScreenshotGalleryService();
        fakeService.StoredItems.Add(new GalleryItem
        {
            FileName = "shot1.png",
            FilePath = @"C:\FakePictures\NeatShot\shot1.png",
            CreatedAt = DateTime.Now
        });
        fakeService.StoredItems.Add(new GalleryItem
        {
            FileName = "shot2.png",
            FilePath = @"C:\FakePictures\NeatShot\shot2.png",
            CreatedAt = DateTime.Now.AddMinutes(-5)
        });

        var vm = new GalleryViewModel(fakeService);
        await vm.LoadItemsAsync();

        Assert.Equal(2, vm.Items.Count);
        Assert.True(vm.HasItems);
        Assert.False(vm.IsEmpty);
        Assert.False(vm.IsLoading);
        Assert.Equal(@"C:\FakePictures\NeatShot", vm.ScreenshotsDirectory);
    }

    [Fact]
    public async Task PinItem_FiresPinRequestedEvent_WhenImageLoadsSuccessfully()
    {
        var dummyBitmap = CreateDummyBitmap();
        var fakeService = new FakeScreenshotGalleryService
        {
            FakeLoadedBitmap = dummyBitmap
        };

        var item = new GalleryItem
        {
            FileName = "shot1.png",
            FilePath = @"C:\FakePictures\NeatShot\shot1.png"
        };
        fakeService.StoredItems.Add(item);

        var vm = new GalleryViewModel(fakeService);
        await vm.LoadItemsAsync();

        BitmapSource? receivedBitmap = null;
        vm.PinRequested += (s, bmp) =>
        {
            receivedBitmap = bmp;
        };

        vm.PinItem(item);

        Assert.NotNull(receivedBitmap);
        Assert.Same(dummyBitmap, receivedBitmap);
    }

    [Fact]
    public async Task DeleteItemAsync_RemovesItemFromCollection_WhenDeletionSucceeds()
    {
        var fakeService = new FakeScreenshotGalleryService();
        var item1 = new GalleryItem
        {
            FileName = "shot1.png",
            FilePath = @"C:\FakePictures\NeatShot\shot1.png"
        };
        var item2 = new GalleryItem
        {
            FileName = "shot2.png",
            FilePath = @"C:\FakePictures\NeatShot\shot2.png"
        };
        fakeService.StoredItems.Add(item1);
        fakeService.StoredItems.Add(item2);

        var vm = new GalleryViewModel(fakeService);
        await vm.LoadItemsAsync();

        Assert.Equal(2, vm.Items.Count);

        var deleted = await vm.DeleteItemAsync(item1);

        Assert.True(deleted);
        Assert.Single(vm.Items);
        Assert.Equal("shot2.png", vm.Items[0].FileName);
    }

    [Fact]
    public void OpenFolder_CallsConfiguredAction()
    {
        var fakeService = new FakeScreenshotGalleryService();
        var vm = new GalleryViewModel(fakeService);

        string? openedPath = null;
        vm.FolderOpenerAction = path => openedPath = path;

        vm.OpenFolder();

        Assert.Equal(fakeService.FakeDirectory, openedPath);
    }
}
