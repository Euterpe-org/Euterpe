using Avalonia.Platform.Storage;

namespace Euterpe.Core.Proxies;

[UsedImplicitly]
public sealed class TopLevelProxy
{
    public required Func<TopLevel> TopLevelFactory { get; init; }
    private TopLevel Current => TopLevelFactory();

    public IStorageProvider StorageProvider => Current.StorageProvider;
    public ILauncher Launcher => Current.Launcher;
}