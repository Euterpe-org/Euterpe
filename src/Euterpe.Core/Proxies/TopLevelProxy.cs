using Avalonia.Platform.Storage;

namespace Euterpe.Core.Proxies;

public sealed class TopLevelProxy
{
    [UsedImplicitly]
    public required Func<TopLevel> TopLevelFactory { get; init; }

    private TopLevel Current => TopLevelFactory();

    public IStorageProvider StorageProvider => Current.StorageProvider;
    public ILauncher Launcher => Current.Launcher;
}