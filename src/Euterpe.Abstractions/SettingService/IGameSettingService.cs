namespace Euterpe.Abstractions;

public interface IGameSettingService
{
    bool IsValidGameFolder();

    void EnsureGameFolders();
}