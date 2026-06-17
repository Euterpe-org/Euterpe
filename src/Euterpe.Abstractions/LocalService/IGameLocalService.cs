namespace Euterpe.Abstractions;

public interface IGameLocalService
{
    Task<string> GetGameFolderAsync();
    Task InstallMelonLoaderAsync();
    Task UninstallMelonLoaderAsync();
    void ReadGameInformation();
    void ReadMelonLoaderVersion();
}
