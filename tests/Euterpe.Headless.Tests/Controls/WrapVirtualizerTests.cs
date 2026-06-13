using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(WrapVirtualizer))]
public sealed class WrapVirtualizerTests : HeadlessTest
{
    private static (WrapVirtualizer Wrap, Window Window) CreateHost(int itemCount, double itemWidth, double windowWidth, double windowHeight)
    {
        var wrap = new WrapVirtualizer
        {
            ItemsSource = Enumerable.Range(0, itemCount).Select(i => $"item {i}").ToList(),
            ItemWidth = itemWidth,
            ItemTemplate = new FuncDataTemplate<string>((_, _) => new Border { Width = itemWidth, Height = 40 }, true)
        };
        var window = new Window { Content = wrap, Width = windowWidth, Height = windowHeight };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (wrap, window);
    }

    [Test]
    public Task Rows_ChunkItemsByComputedColumnCount() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(itemCount: 10, itemWidth: 100, windowWidth: 350, windowHeight: 400);

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(3);
        await Assert.That(wrap.Rows.Count).IsEqualTo(4);
    });

    [Test]
    public Task Rows_LastRowHoldsRemainder() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(itemCount: 10, itemWidth: 100, windowWidth: 350, windowHeight: 400);
        await Assert.That(wrap.Rows[^1].Items.Count).IsEqualTo(1);
    });

    [Test]
    public Task Virtualization_RealizesFewerRowsThanTotal() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(itemCount: 2000, itemWidth: 100, windowWidth: 350, windowHeight: 400);

        var panel = wrap.GetVisualDescendants().OfType<VirtualizingStackPanel>().First();
        var realizedRows = panel.GetVisualChildren().OfType<Control>().Count();

        using var _ = Assert.Multiple();
        await Assert.That(wrap.Rows.Count).IsEqualTo(667);
        await Assert.That(realizedRows).IsGreaterThan(0);
        await Assert.That(realizedRows).IsLessThan(wrap.Rows.Count);
    });

    [Test]
    public Task WidthChange_RecomputesColumns() => RunOnUI(async () =>
    {
        var (wrap, window) = CreateHost(itemCount: 12, itemWidth: 100, windowWidth: 350, windowHeight: 400);
        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(3);

        window.Width = 520;
        Dispatcher.UIThread.RunJobs();

        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(5);
    });

    [Test]
    public Task ItemTemplate_RendersItemsInRow() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(itemCount: 6, itemWidth: 100, windowWidth: 350, windowHeight: 400);

        var borders = wrap.GetVisualDescendants().OfType<Border>().Count(b => b.Width == 100 && b.Height == 40);
        await Assert.That(borders).IsGreaterThan(0);
    });

    [Test]
    public Task Columns_ComputedFromAvailableWidth_NotArrangedWidth() => RunOnUI(async () =>
    {
        var wrap = new WrapVirtualizer
        {
            ItemsSource = Enumerable.Range(0, 10).Select(i => $"item {i}").ToList(),
            ItemWidth = 100,
            ItemTemplate = new FuncDataTemplate<string>((_, _) => new Border { Width = 100, Height = 40 }, true)
        };
        var host = new ContentControl
        {
            Content = wrap,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Top,
            Width = 350
        };
        var window = new Window { Content = host, Width = 600, Height = 400 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await Assert.That(wrap.Rows[0].Items.Count).IsEqualTo(3);
    });

    [Test]
    public Task Cards_InSameRowAreArrangedHorizontally() => RunOnUI(async () =>
    {
        var (wrap, _) = CreateHost(itemCount: 6, itemWidth: 100, windowWidth: 350, windowHeight: 400);

        var cards = wrap.GetVisualDescendants()
            .OfType<Border>()
            .Where(b => b.Width == 100 && b.Height == 40)
            .Take(2)
            .ToList();
        var first = cards[0].TranslatePoint(default, wrap) ?? default;
        var second = cards[1].TranslatePoint(default, wrap) ?? default;

        using var _ = Assert.Multiple();
        await Assert.That(second.X).IsGreaterThan(first.X);
        await Assert.That(second.Y).IsEqualTo(first.Y);
    });
}
