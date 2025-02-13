using Avalonia.Platform;

namespace MuseDashModTools.Services;

public sealed class ResourceService : IResourceService
{
    public Stream GetAssetAsStream(string fileName) => AssetLoader.Open(new Uri($"avares://{nameof(MuseDashModTools)}/Assets/{fileName}"));

    public T? TryGetAppResource<T>(string key) where T : class
    {
        if (!GetCurrentApplication().TryGetResource(key, out var result))
        {
            return null;
        }

        return result as T;
    }
}