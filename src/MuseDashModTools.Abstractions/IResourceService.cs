namespace MuseDashModTools.Abstractions;

public interface IResourceService
{
    Stream GetAssetAsStream(string fileName);
    T? TryGetAppResource<T>(string key) where T : class;
}