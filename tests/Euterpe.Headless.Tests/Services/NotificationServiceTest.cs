using Euterpe.Core;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Headless.Tests.Services;

/// <summary>
///     Each test calls <c>manager.CloseAll()</c> before exiting <c>RunOnUI</c>. This pre-marks every
///     <c>NotificationCard</c> as <c>IsClosing = true</c>, so when Ursa's hidden 3-second
///     <c>await Task.Delay</c> continuation fires later (after the test session has torn down its
///     dispatcher and the captured sync context is dead, so the continuation falls back to the
///     threadpool), <c>MessageCard.Close()</c>'s <c>if (!IsClosing)</c> guard short-circuits — no
///     <c>SetAndRaise</c>, no <c>VerifyAccess</c>, no AppDomain crash. Linux didn't reproduce, but
///     the race is real on Windows headless CI.
/// </summary>
[TestSubject(typeof(NotificationService))]
public sealed class NotificationServiceTest : HeadlessTest
{
    private static (NotificationService service, WindowNotificationManager manager, Func<int> factoryCalls) NewWiredService()
    {
        var window = new Window();
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var manager = new WindowNotificationManager(window);
        var calls = 0;
        var service = new NotificationService
        {
            NotificationManagerFactory = () =>
            {
                calls++;
                return manager;
            }
        };
        return (service, manager, () => calls);
    }

    [Test]
    public Task Success_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, manager, calls) = NewWiredService();

        service.Success("hello");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
        manager.CloseAll();
    });

    [Test]
    public Task Notice_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, manager, calls) = NewWiredService();

        service.Notice("hello");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
        manager.CloseAll();
    });

    [Test]
    public Task Warning_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, manager, calls) = NewWiredService();

        service.Warning("hello");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
        manager.CloseAll();
    });

    [Test]
    public Task Error_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, manager, calls) = NewWiredService();

        service.Error("hello");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
        manager.CloseAll();
    });

    [Test]
    public Task LightVariants_RouteThroughFactory() => RunOnUI(async () =>
    {
        var (service, manager, calls) = NewWiredService();

        service.SuccessLight("a");
        service.NoticeLight("b");
        service.WarningLight("c");
        service.ErrorLight("d");
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(4);
        manager.CloseAll();
    });

    [Test]
    public Task FormatStringOverloads_FormatAndRoute() => RunOnUI(async () =>
    {
        var (service, manager, calls) = NewWiredService();

        service.Success("count={0}", 1);
        service.Notice("count={0}", 2);
        service.Warning("count={0}", 3);
        service.Error("count={0}", 4);
        service.SuccessLight("count={0}", 5);
        service.NoticeLight("count={0}", 6);
        service.WarningLight("count={0}", 7);
        service.ErrorLight("count={0}", 8);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(8);
        manager.CloseAll();
    });

    [Test]
    public Task FormatString_InvalidFormat_Throws() => RunOnUI(async () =>
    {
        var (service, manager, _) = NewWiredService();

        var act = () => service.Success("missing arg {0} {1}", 42);
        await Assert.That(act).Throws<FormatException>();
        manager.CloseAll();
    });

    [Test]
    public async Task OffUIThread_DispatchesToUIThread()
    {
        var manager = await RunOnUI(() =>
        {
            var window = new Window();
            window.Show();
            Dispatcher.UIThread.RunJobs();
            return Task.FromResult(new WindowNotificationManager(window));
        });

        var calls = 0;
        var service = new NotificationService
        {
            NotificationManagerFactory = () =>
            {
                calls++;
                return manager;
            }
        };

        // Call from a thread that is NOT the UI thread.
        await Task.Run(() => service.Success("from background"));

        await RunOnUI(async () =>
        {
            Dispatcher.UIThread.RunJobs();
            await Assert.That(calls).IsEqualTo(1);
            manager.CloseAll();
        });
    }
}
