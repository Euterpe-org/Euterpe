using Avalonia.Platform.Storage;

namespace Euterpe.Core.Extensions;

public static class StorageItemExtensions
{
    public static IEnumerable<string?> GetLocalPaths(this IReadOnlyList<IStorageItem> items) =>
        items.Select(item => item.TryGetLocalPath());
}
