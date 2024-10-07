namespace MuseDashModTools.Abstractions;

public interface IResourceService
{
    Stream GetAssetAsStream(string fileName);
}