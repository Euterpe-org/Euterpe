using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(CoverImage))]
[Category("CoverImageTests")]
public sealed class CoverImageTest : HeadlessTest
{
    private static readonly byte[] MinimalGif =
        Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

    private static readonly byte[] MinimalWebp =
        Convert.FromBase64String("UklGRjwAAABXRUJQVlA4IDAAAADQAQCdASoEAAQAAgA0JaACdLoB+AADsAD+8Oj3/yC5YXXI1/8gP+QH/ID/+PIAAAA=");

    [Test]
    public Task DefaultStretch_IsUniform() => RunOnUI(async () =>
    {
        var cover = new CoverImage();
        await Assert.That(cover.Stretch).IsEqualTo(Stretch.Uniform);
    });

    [Test]
    public Task ApplyTemplate_CreatesStaticAndGifParts() => RunOnUI(async () =>
    {
        var cover = Show(new CoverImage());

        using var _ = Assert.Multiple();
        await Assert.That(StaticPart(cover)).IsNotNull();
        await Assert.That(GifPart(cover)).IsNotNull();
    });

    [Test]
    public Task NullSource_DoesNotThrowAfterTemplateApply() => RunOnUI(async () =>
    {
        var cover = Show(new CoverImage { Source = null });
        await Assert.That(cover.Source).IsNull();
    });

    [Test]
    public Task StaticSource_ShowsStaticHidesGif() => RunOnUI(async () =>
    {
        var path = CreateTempPng(64, 64);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var cover = Show(new CoverImage { Source = uri });

            using var _ = Assert.Multiple();
            await Assert.That(StaticPart(cover).IsVisible).IsTrue();
            await Assert.That(GifPart(cover).IsVisible).IsFalse();
            await Assert.That(StaticPart(cover).Source).IsEqualTo(uri);
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task GifSource_ShowsGifHidesStatic() => RunOnUI(async () =>
    {
        var path = CreateTempGif();
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(path).AbsoluteUri });
            var source = await WaitForGifSource(cover);

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(GifPart(cover).IsVisible).IsTrue();
            await Assert.That(StaticPart(cover).IsVisible).IsFalse();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task GifSourceChange_DisposesPreviousGifSource() => RunOnUI(async () =>
    {
        var pathA = CreateTempGif();
        var pathB = CreateTempGif();
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(pathA).AbsoluteUri });
            var first = await WaitForGifSource(cover);

            cover.Source = new Uri(pathB).AbsoluteUri;
            var second = await WaitForGifSource(cover, first);

            using var _ = Assert.Multiple();
            await Assert.That(first).IsNotNull();
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
    public Task DetachFromVisualTree_DisposesGifSourceAndClearsSource() => RunOnUI(async () =>
    {
        var path = CreateTempGif();
        try
        {
            var cover = new CoverImage { Source = new Uri(path).AbsoluteUri };
            var window = new Window { Content = cover, Width = 200, Height = 200 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var source = await WaitForGifSource(cover);
            var gifPart = GifPart(cover);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(IsDisposed(source!)).IsTrue();
            await Assert.That(gifPart.Source).IsNull();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task SwitchFromGifToStatic_DisposesGifSource() => RunOnUI(async () =>
    {
        var gifPath = CreateTempGif();
        var pngPath = CreateTempPng(64, 64);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(gifPath).AbsoluteUri });
            var source = await WaitForGifSource(cover);

            cover.Source = new Uri(pngPath).AbsoluteUri;
            Dispatcher.UIThread.RunJobs();

            using var _ = Assert.Multiple();
            await Assert.That(source).IsNotNull();
            await Assert.That(IsDisposed(source!)).IsTrue();
            await Assert.That(StaticPart(cover).IsVisible).IsTrue();
            await Assert.That(GifPart(cover).IsVisible).IsFalse();
        }
        finally
        {
            File.Delete(gifPath);
            File.Delete(pngPath);
        }
    });

    [Test]
    public Task InvalidGif_FallsBackToStatic() => RunOnUI(async () =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"euterpe_coverimage_{Guid.NewGuid():N}.gif");
        File.WriteAllBytes(path, "NOT A GIF, NOT A GIF, NOT A GIF"u8.ToArray());
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var cover = Show(new CoverImage { Source = uri });
            await WaitUntil(() => StaticPart(cover).IsVisible);

            using var _ = Assert.Multiple();
            await Assert.That(StaticPart(cover).IsVisible).IsTrue();
            await Assert.That(StaticPart(cover).Source).IsEqualTo(uri);
            await Assert.That(GifPart(cover).IsVisible).IsFalse();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task GifReattach_RebuildsGifSource() => RunOnUI(async () =>
    {
        var path = CreateTempGif();
        try
        {
            var cover = new CoverImage { Source = new Uri(path).AbsoluteUri };
            var window = new Window { Content = cover, Width = 200, Height = 200 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var first = await WaitForGifSource(cover);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Content = cover;
            Dispatcher.UIThread.RunJobs();

            var reloaded = await WaitForGifSource(cover);

            using var _ = Assert.Multiple();
            await Assert.That(first).IsNotNull();
            await Assert.That(reloaded).IsNotNull();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task StaticSource_DecodesBitmapIntoStaticPart() => RunOnUI(async () =>
    {
        var path = CreateTempPng(128, 128);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(path).AbsoluteUri, DecodeWidth = 64, Stretch = Stretch.UniformToFill });
            var bitmap = await WaitForStaticBitmap(cover);

            await Assert.That(bitmap).IsNotNull();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task BoundSourceInItemTemplate_DecodesStaticBitmap() => RunOnUI(async () =>
    {
        var path = CreateTempPng(128, 128);
        try
        {
            var items = new ItemsControl
            {
                ItemsSource = new[] { new CoverData { CoverPath = new Uri(path).AbsoluteUri } },
                ItemTemplate = new FuncDataTemplate<CoverData>((_, _) =>
                {
                    var cover = new CoverImage { DecodeWidth = 64, Stretch = Stretch.UniformToFill };
                    cover.Bind(CoverImage.SourceProperty, new Binding(nameof(CoverData.CoverPath)));
                    return cover;
                })
            };
            var window = new Window { Content = items, Width = 200, Height = 200 };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cover = items.GetVisualDescendants().OfType<CoverImage>().First();
            var bitmap = await WaitForStaticBitmap(cover);

            await Assert.That(bitmap).IsNotNull();
        }
        finally
        {
            File.Delete(path);
        }
    });

    [Test]
    public Task WebpSource_DecodesIntoStaticPart() => RunOnUI(async () =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"euterpe_coverimage_{Guid.NewGuid():N}.webp");
        File.WriteAllBytes(path, MinimalWebp);
        try
        {
            var cover = Show(new CoverImage { Source = new Uri(path).AbsoluteUri, DecodeWidth = 4, Stretch = Stretch.UniformToFill });
            var bitmap = await WaitForStaticBitmap(cover);

            using var _ = Assert.Multiple();
            await Assert.That(bitmap).IsNotNull();
            await Assert.That(StaticPart(cover).IsVisible).IsTrue();
            await Assert.That(GifPart(cover).IsVisible).IsFalse();
        }
        finally
        {
            File.Delete(path);
        }
    });

    private static CoverImage Show(CoverImage cover)
    {
        var window = new Window { Content = cover, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return cover;
    }

    private static AsyncImage StaticPart(CoverImage cover) =>
        cover.GetVisualDescendants().OfType<AsyncImage>().First(part => part.Name is "PART_Static");

    private static GifImage GifPart(CoverImage cover) =>
        cover.GetVisualDescendants().OfType<GifImage>().First(part => part.Name is "PART_Gif");

    private static async Task<IGifSource?> WaitForGifSource(CoverImage cover, IGifSource? previous = null)
    {
        var gifPart = GifPart(cover);
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (gifPart.Source is { } source && !ReferenceEquals(source, previous))
            {
                return source;
            }

            await Task.Delay(5);
        }

        return gifPart.Source;
    }

    private static async Task<Bitmap?> WaitForStaticBitmap(CoverImage cover)
    {
        var staticPart = StaticPart(cover);
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            var inner = staticPart.GetVisualDescendants().OfType<Image>().FirstOrDefault(image => image.Name is "PART_Image");
            if (inner?.Source is Bitmap bitmap)
            {
                return bitmap;
            }

            await Task.Delay(5);
        }

        return null;
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }

            await Task.Delay(5);
        }
    }

    private static bool IsDisposed(IGifSource source)
    {
        try
        {
            source.GetStream();
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    private static string CreateTempGif()
    {
        var path = Path.Combine(Path.GetTempPath(), $"euterpe_coverimage_{Guid.NewGuid():N}.gif");
        File.WriteAllBytes(path, MinimalGif);
        return path;
    }

    private static string CreateTempPng(int width, int height)
    {
        using var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        var path = Path.Combine(Path.GetTempPath(), $"euterpe_coverimage_{Guid.NewGuid():N}.png");
        using var stream = File.Create(path);
        bitmap.Save(stream, new PngBitmapEncoderOptions());
        return path;
    }

    private sealed class CoverData
    {
        public string? CoverPath { get; init; }
    }
}
