using Avalonia.Platform;

namespace Euterpe.Core;

internal sealed class ResourceService : IResourceService
{
    public Stream GetAssetAsStream(string fileName) => AssetLoader.Open(new Uri($"avares://{nameof(Euterpe)}/Assets/{fileName}"));

    public T? TryGetAppResource<T>(string key) where T : class
    {
        if (!GetCurrentApplication().TryGetResource(key, out var result))
        {
            return null;
        }

        return result as T;
    }
}