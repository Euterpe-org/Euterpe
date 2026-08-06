using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace Euterpe.Headless.Tests.Theme;

[NotInParallel("ApplicationThemeVariant")]
[Category("ThemeSwitchingTests")]
public sealed class ThemeSwitchingTest : HeadlessTest
{
    [Test]
    public Task FindResource_Dark_ResolvesBackgroundImage() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var resource = window.FindResource(ThemeVariant.Dark, "BackgroundImage");

            using var _ = Assert.Multiple();
            await Assert.That(resource).IsNotNull();
            await Assert.That(resource).IsTypeOf<Bitmap>();
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_Light_ResolvesBackgroundImage() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var resource = window.FindResource(ThemeVariant.Light, "BackgroundImage");

            using var _ = Assert.Multiple();
            await Assert.That(resource).IsNotNull();
            await Assert.That(resource).IsTypeOf<Bitmap>();
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_DarkVsLight_BackgroundImageIsShared() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var darkBg = window.FindResource(ThemeVariant.Dark, "BackgroundImage");
            var lightBg = window.FindResource(ThemeVariant.Light, "BackgroundImage");

            await Assert.That(darkBg).IsSameReferenceAs(lightBg);
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_DarkVsLight_HomeScrimBrushesDiffer() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var darkScrim = window.FindResource(ThemeVariant.Dark, "HomeScrimBrush");
            var lightScrim = window.FindResource(ThemeVariant.Light, "HomeScrimBrush");

            using var _ = Assert.Multiple();
            await Assert.That(darkScrim).IsTypeOf<LinearGradientBrush>();
            await Assert.That(lightScrim).IsTypeOf<LinearGradientBrush>();
            await Assert.That(darkScrim).IsNotSameReferenceAs(lightScrim);
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_DarkVsLight_TitleEdgePaintsOnLightOnly() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var dark = window.FindResource(ThemeVariant.Dark, "HomeTitleEffect") as DropShadowEffect;
            var light = window.FindResource(ThemeVariant.Light, "HomeTitleEffect") as DropShadowEffect;

            using var _ = Assert.Multiple();
            await Assert.That(dark).IsNotNull();
            await Assert.That(light).IsNotNull();
            await Assert.That(dark!.Color.A).IsEqualTo<byte>(0);
            await Assert.That(light!.Color.A).IsEqualTo<byte>(255);
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task FindResource_DarkVsLight_PrimaryTitleColorsDiffer() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var darkColor = window.FindResource(ThemeVariant.Dark, "PrimaryTitleColor");
            var lightColor = window.FindResource(ThemeVariant.Light, "PrimaryTitleColor");

            using var _ = Assert.Multiple();
            await Assert.That(darkColor).IsTypeOf<Color>();
            await Assert.That(lightColor).IsTypeOf<Color>();
            await Assert.That(darkColor).IsNotEqualTo(lightColor);
        }
        finally
        {
            window.Close();
        }
    });

    [Test]
    public Task SharedResource_CheckMark_ResolvesRegardlessOfTheme() => RunOnUI(async () =>
    {
        var window = NewWindow();
        try
        {
            var darkCheck = window.FindResource(ThemeVariant.Dark, "CheckMark");
            var lightCheck = window.FindResource(ThemeVariant.Light, "CheckMark");

            using var _ = Assert.Multiple();
            await Assert.That(darkCheck).IsNotNull();
            await Assert.That(lightCheck).IsNotNull();
            await Assert.That(darkCheck).IsSameReferenceAs(lightCheck);
        }
        finally
        {
            window.Close();
        }
    });

    private static Window NewWindow()
    {
        var window = new Window { Width = 100, Height = 100 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
