namespace Euterpe.Headless.Tests;

public static class GlobalHooks
{
    [After(TestSession)]
    public static ValueTask DisposeHeadlessSession() =>
        HeadlessTest.SessionLazy.IsValueCreated
            ? HeadlessTest.SessionLazy.Value.DisposeAsync()
            : ValueTask.CompletedTask;
}