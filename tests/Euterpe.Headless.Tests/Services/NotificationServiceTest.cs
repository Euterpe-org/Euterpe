using Euterpe.Core;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(NotificationService))]
public sealed class NotificationServiceTest : HeadlessTest
{
    private static (NotificationService service, Func<int> factoryCalls) NewWiredService()
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
        return (service, () => calls);
    }

    [Test]
    public Task Success_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, calls) = NewWiredService();

        service.Success("hello", TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
    });

    [Test]
    public Task Notice_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, calls) = NewWiredService();

        service.Notice("hello", TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
    });

    [Test]
    public Task Warning_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, calls) = NewWiredService();

        service.Warning("hello", TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
    });

    [Test]
    public Task Error_RoutesThroughFactory() => RunOnUI(async () =>
    {
        var (service, calls) = NewWiredService();

        service.Error("hello", TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(1);
    });

    [Test]
    public Task LightVariants_RouteThroughFactory() => RunOnUI(async () =>
    {
        var (service, calls) = NewWiredService();

        service.SuccessLight("a", TimeSpan.Zero);
        service.NoticeLight("b", TimeSpan.Zero);
        service.WarningLight("c", TimeSpan.Zero);
        service.ErrorLight("d", TimeSpan.Zero);
        Dispatcher.UIThread.RunJobs();

        await Assert.That(calls()).IsEqualTo(4);
    });

    [Test]
    public Task FormatString_InvalidFormat_Throws() => RunOnUI(async () =>
    {
        var (service, _) = NewWiredService();

        var act = () => service.Success("missing arg {0} {1}", 42);
        await Assert.That(act).Throws<FormatException>();
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
        await Task.Run(() => service.Success("from background", TimeSpan.Zero));

        await RunOnUI(async () =>
        {
            Dispatcher.UIThread.RunJobs();
            await Assert.That(calls).IsEqualTo(1);
        });
    }
}