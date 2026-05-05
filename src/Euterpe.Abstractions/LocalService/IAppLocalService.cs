namespace Euterpe.Abstractions;

public interface IAppLocalService
{
    Task<string> GetSteamFolderAsync();
    Task<string> GetSteamExecPathAsync();
    Task<string> GetCacheFolderAsync();
}