using Avalonia.Media.Imaging;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(AsyncImage))]
public sealed class AsyncImageTests : HeadlessTest
{
    [Test]
    public Task DefaultStretch_IsUniform() => RunOnUI(async () =>
    {
        var image = new AsyncImage();
        await Assert.That(image.Stretch).IsEqualTo(Stretch.Uniform);
    });

    [Test]
    public Task ApplyTemplate_CreatesPartImageAndPartPlaceholder() => RunOnUI(async () =>
    {
        var asyncImage = new AsyncImage();
        var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var partImage = asyncImage.GetVisualDescendants()
            .OfType<Image>()
            .FirstOrDefault(i => i.Name is "PART_Image");
        var partPlaceholder = asyncImage.GetVisualDescendants()
            .OfType<Image>()
            .FirstOrDefault(i => i.Name is "PART_PlaceholderImage");

        using var _ = Assert.Multiple();
        await Assert.That(partImage).IsNotNull();
        await Assert.That(partPlaceholder).IsNotNull();
    });

    [Test]
    public Task NullSource_DoesNotThrowAfterTemplateApply() => RunOnUI(async () =>
    {
        var asyncImage = new AsyncImage { Source = null };
        var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await Assert.That(asyncImage.Source).IsNull();
    });

    [Test]
    public Task EmptySource_DoesNotThrowAfterTemplateApply() => RunOnUI(async () =>
    {
        var asyncImage = new AsyncImage { Source = string.Empty };
        var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await Assert.That(asyncImage.Source).IsEqualTo(string.Empty);
    });

    [Test]
    public Task InvalidUri_DoesNotThrowAfterTemplateApply() => RunOnUI(async () =>
    {
        var asyncImage = new AsyncImage { Source = "not-a-uri" };
        var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await Assert.That(asyncImage.Source).IsEqualTo("not-a-uri");
    });

    [Test]
    public Task StretchChange_PropagatesToPartImage() => RunOnUI(async () =>
    {
        var asyncImage = new AsyncImage { Stretch = Stretch.Fill };
        var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var partImage = asyncImage.GetVisualDescendants()
            .OfType<Image>()
            .First(i => i.Name is "PART_Image");
        await Assert.That(partImage.Stretch).IsEqualTo(Stretch.Fill);
    });

    [Test]
    public Task DecodeWidth_DecodesSourceToConfiguredWidth() => RunOnUI(async () =>
    {
        var path = CreateTempPng(512, 512);
        try
        {
            var asyncImage = new AsyncImage { Source = new Uri(path).AbsoluteUri, DecodeWidth = 64 };
            var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
            window.Show();

            var bitmap = await WaitForBitmap(asyncImage);

            using var _ = Assert.Multiple();
            await Assert.That(bitmap).IsNotNull();
            await Assert.That(bitmap!.PixelSize.Width).IsEqualTo(64);
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task SourceChange_DisposesPreviousBitmap() => RunOnUI(async () =>
    {
        var pathA = CreateTempPng(256, 256);
        var pathB = CreateTempPng(256, 256);
        try
        {
            var asyncImage = new AsyncImage { Source = new Uri(pathA).AbsoluteUri, DecodeWidth = 64 };
            var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
            window.Show();
            var first = await WaitForBitmap(asyncImage);

            asyncImage.Source = new Uri(pathB).AbsoluteUri;
            var second = await WaitForBitmap(asyncImage, first);

            using var _ = Assert.Multiple();
            await Assert.That(second).IsNotNull();
            await Assert.That(IsDisposed(first!)).IsTrue();
        }
        finally
        {
            File.Delete(pathA);
            File.Delete(pathB);
        }
    });

    [Test]
    public Task DetachFromVisualTree_DisposesBitmapAndClearsSource() => RunOnUI(async () =>
    {
        var path = CreateTempPng(256, 256);
        try
        {
            var asyncImage = new AsyncImage { Source = new Uri(path).AbsoluteUri, DecodeWidth = 64 };
            var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
            window.Show();
            var bitmap = await WaitForBitmap(asyncImage);
            var partImage = PartImage(asyncImage);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            using var _ = Assert.Multiple();
            await Assert.That(IsDisposed(bitmap!)).IsTrue();
            await Assert.That(partImage.Source).IsNull();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task ReattachWithUnchangedSource_ReloadsBitmap() => RunOnUI(async () =>
    {
        var path = CreateTempPng(256, 256);
        try
        {
            var asyncImage = new AsyncImage { Source = new Uri(path).AbsoluteUri, DecodeWidth = 64 };
            var window = new Window { Content = asyncImage, Width = 200, Height = 200 };
            window.Show();
            await WaitForBitmap(asyncImage);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Content = asyncImage;
            Dispatcher.UIThread.RunJobs();

            var reloaded = await WaitForBitmap(asyncImage);
            await Assert.That(reloaded).IsNotNull();
        }
        finally
        {
            File.Delete(path);
        }
    });

    private static Image PartImage(AsyncImage image) =>
        image.GetVisualDescendants().OfType<Image>().First(i => i.Name is "PART_Image");

    private static async Task<Bitmap?> WaitForBitmap(AsyncImage image, Bitmap? previous = null)
    {
        var partImage = PartImage(image);
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (partImage.Source is Bitmap bitmap && !ReferenceEquals(bitmap, previous))
            {
                return bitmap;
            }

            await Task.Delay(5);
        }

        return partImage.Source as Bitmap;
    }

    private static bool IsDisposed(Bitmap bitmap)
    {
        try
        {
            _ = bitmap.PixelSize;
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    private static string CreateTempPng(int width, int height)
    {
        using var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        var path = Path.Combine(Path.GetTempPath(), $"euterpe_asyncimage_{Guid.NewGuid():N}.png");
        using var stream = File.Create(path);
        bitmap.Save(stream);
        return path;
    }
}
