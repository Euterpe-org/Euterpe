namespace Euterpe.Tests.Core;

[Category("FolderWatcherTests")]
[TestSubject(typeof(FolderWatcher))]
public sealed partial class FolderWatcherTest
{
    private static readonly TimeSpan ReconcileTimeout = TimeSpan.FromSeconds(10);
}
