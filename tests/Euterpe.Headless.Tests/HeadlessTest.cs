namespace Euterpe.Headless.Tests;

public abstract class HeadlessTest
{
    internal static readonly Lazy<HeadlessUnitTestSession> SessionLazy =
        new(() => HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder)));

    protected static HeadlessUnitTestSession Session => SessionLazy.Value;

    protected static Task RunOnUI(Action action) =>
        Session.Dispatch(action, CancellationToken.None);

    protected static Task RunOnUI(Func<Task> func) =>
        Session.Dispatch(
            async () =>
            {
                await func();
                return 0;
            },
            CancellationToken.None);

    protected static Task<T> RunOnUI<T>(Func<Task<T>> func) =>
        Session.Dispatch(func, CancellationToken.None);
}