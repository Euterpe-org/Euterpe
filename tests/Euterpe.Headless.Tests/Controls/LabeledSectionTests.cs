using Avalonia.VisualTree;
using Euterpe.Controls;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(LabeledSection))]
public sealed class LabeledSectionTests : HeadlessTest
{
    [Test]
    public Task Title_and_description_bind_to_template_textblocks() => RunOnUI(async () =>
    {
        var section = new LabeledSection { Title = "Hello", Description = "World" };
        var window = new Window { Content = section, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var textBlocks = section.GetVisualDescendants().OfType<TextBlock>().ToList();
        using var _ = Assert.Multiple();
        await Assert.That(textBlocks).Count().IsEqualTo(2);
        await Assert.That(textBlocks[0].Text).IsEqualTo("Hello");
        await Assert.That(textBlocks[1].Text).IsEqualTo("World");
    });

    [Test]
    public Task Title_change_propagates_to_template_textblock() => RunOnUI(async () =>
    {
        var section = new LabeledSection { Title = "Initial" };
        var window = new Window { Content = section, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        section.Title = "Updated";
        Dispatcher.UIThread.RunJobs();

        var titleTextBlock = section.GetVisualDescendants().OfType<TextBlock>().First();
        await Assert.That(titleTextBlock.Text).IsEqualTo("Updated");
    });
}