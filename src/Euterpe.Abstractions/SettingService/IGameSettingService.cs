namespace Euterpe.Abstractions;

public interface IGameSettingService
{
    Task ValidateGameFolderAsync();

    void EnsureGameFolders();
}