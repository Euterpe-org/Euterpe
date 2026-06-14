using System.Collections.Specialized;
using System.Reactive.Concurrency;
using DynamicData;
using Euterpe.Reactive;

namespace Euterpe.Headless.Tests.Reactive;

[TestSubject(typeof(AvaloniaScheduler))]
public sealed class AvaloniaSchedulerTests : HeadlessTest
{
    [Test]
    public Task Schedule_OnUIThread_RunsInline() => RunOnUI(async () =>
    {
        var ranSynchronously = false;
        var ranOnUIThread = false;

        AvaloniaScheduler.Instance.Schedule(() =>
        {
            ranSynchronously = true;
            ranOnUIThread = Dispatcher.UIThread.CheckAccess();
        });

        using var _ = Assert.Multiple();
        await Assert.That(ranSynchronously).IsTrue();
        await Assert.That(ranOnUIThread).IsTrue();
    });

    [Test]
    public Task Schedule_OffUIThread_PostsToUIThread() => RunOnUI(async () =>
    {
        var ranOnUIThread = false;

        await Task.Run(() =>
            AvaloniaScheduler.Instance.Schedule(() => ranOnUIThread = Dispatcher.UIThread.CheckAccess()));
        Dispatcher.UIThread.RunJobs();

        await Assert.That(ranOnUIThread).IsTrue();
    });

    [Test]
    public Task SortAndBindOnUI_BackgroundMutation_BindsOnUIThreadInSortedOrder() => RunOnUI(async () =>
    {
        var cache = new SourceCache<Item, int>(item => item.Id);
        using var subscription = cache.Connect()
            .SortAndBindOnUI(out var items, Comparer<Item>.Create((left, right) => left.Id.CompareTo(right.Id)))
            .Subscribe();

        var boundOffUIThread = false;
        ((INotifyCollectionChanged)items).CollectionChanged += (_, _) =>
            boundOffUIThread |= !Dispatcher.UIThread.CheckAccess();

        await Task.Run(() =>
        {
            cache.AddOrUpdate(new Item(2));
            cache.AddOrUpdate(new Item(1));
        });
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(items.Count).IsEqualTo(2);
        await Assert.That(items[0].Id).IsEqualTo(1);
        await Assert.That(items[1].Id).IsEqualTo(2);
        await Assert.That(boundOffUIThread).IsFalse();
    });

    private sealed record Item(int Id);
}
