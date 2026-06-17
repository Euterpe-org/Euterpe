using static Euterpe.IocContainer;

namespace Euterpe;

internal static class CrashHandler
{
    private static int CrashShowing;

    internal static bool ReportException(Exception ex)
    {
        Resolve<ILogger<App>>().ZLogCritical(ex, $"Unhandled exception");

        if (Resolve<Config>().IgnoreException)
        {
            return true;
        }

        if (Interlocked.Exchange(ref CrashShowing, 1) is 1)
        {
            Environment.Exit(1);
            return false;
        }

        Dispatcher.UIThread.Post(() => ShowCrashDialogAsync(ex).SafeFireAndForget());
        return true;
    }

    private static async Task ShowCrashDialogAsync(Exception ex)
    {
        var vm = Resolve<CrashWindowViewModel>();
        vm.SetException(ex);

        try
        {
            var shouldContinue = await Resolve<IDialogService>()
                .ShowWindowDialogAsync<CrashWindow, CrashWindowViewModel, bool>(vm)
                .ConfigureAwait(true);

            if (!shouldContinue)
            {
                Environment.Exit(1);
            }
        }
        catch
        {
            Environment.Exit(1);
        }
        finally
        {
            Interlocked.Exchange(ref CrashShowing, 0);
        }
    }
}
