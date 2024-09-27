using Avalonia.Platform;

namespace MuseDashModTools.Utils;

public static class ResourceUtils
{
    public static Stream GetResource(string fileName) => AssetLoader.Open(new Uri($"avares://{nameof(MuseDashModTools)}/Assets/{fileName}"));
}