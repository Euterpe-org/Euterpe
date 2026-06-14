using System.Collections.Specialized;
using System.Reactive.Concurrency;
using DynamicData;
using Euterpe.Reactive;

namespace Euterpe.Headless.Tests.Reactive;

[TestSubject(typeof(AvaloniaScheduler))]
public sealed class AvaloniaSchedulerTests : HeadlessTest
{
    [Test]
    public Task Schedule_OnUiThread_RunsInline() => RunOnUI(async () =>
    {
        var ranSynchronously = false;
        var ranOnUiThread = false;

        AvaloniaScheduler.Instance.Schedule(() =>
        {
            ranSynchronously = true;
            ranOnUiThread = Dispatcher.UIThread.CheckAccess();
        });

        using var _ = Assert.Multiple();
        await Assert.That(ranSynchronously).IsTrue();
        await Assert.That(ranOnUiThread).IsTrue();
    });

    [Test]
    public Task Schedule_OffUiThread_PostsToUiThread() => RunOnUI(async () =>
    {
        var ranOnUiThread = false;

        await Task.Run(() =>
            AvaloniaScheduler.Instance.Schedule(() => ranOnUiThread = Dispatcher.UIThread.CheckAccess()));
        Dispatcher.UIThread.RunJobs();

        await Assert.That(ranOnUiThread).IsTrue();
    });

    [Test]
    public Task SortAndBindOnUi_BackgroundMutation_BindsOnUiThreadInSortedOrder() => RunOnUI(async () =>
    {
        var cache = new SourceCache<Item, int>(item => item.Id);
        using var subscription = cache.Connect()
            .SortAndBindOnUi(out var items, Comparer<Item>.Create((left, right) => left.Id.CompareTo(right.Id)))
            .Subscribe();

        var boundOffUiThread = false;
        ((INotifyCollectionChanged)items).CollectionChanged += (_, _) =>
            boundOffUiThread |= !Dispatcher.UIThread.CheckAccess();

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
        await Assert.That(boundOffUiThread).IsFalse();
    });

    private sealed record Item(int Id);
}
