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
            .FirstOrDefault(i => i.Name == "PART_Image");
        var partPlaceholder = asyncImage.GetVisualDescendants()
            .OfType<Image>()
            .FirstOrDefault(i => i.Name == "PART_PlaceholderImage");

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
            .First(i => i.Name == "PART_Image");
        await Assert.That(partImage.Stretch).IsEqualTo(Stretch.Fill);
    });
}