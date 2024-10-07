using Avalonia.Platform;

namespace MuseDashModTools.Services;

public class ResourceService: IResourceService
{
    public Stream GetAssetAsStream(string fileName) => AssetLoader.Open(new Uri($"avares://{nameof(MuseDashModTools)}/Assets/{fileName}"));
}