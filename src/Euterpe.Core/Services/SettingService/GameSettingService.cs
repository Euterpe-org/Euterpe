namespace Euterpe.Core;

internal sealed class GameSettingService : IGameSettingService
{
    public bool IsValidGameFolder() => GamePaths.CheckIsValidGameFolder(GameConfig.Folder);

    public void EnsureGameFolders()
    {
        Directory.CreateDirectory(GameConfig.ModsFolder);
        Directory.CreateDirectory(GameConfig.UserLibsFolder);
        Directory.CreateDirectory(GameConfig.OnlineChartsFolder);
        Directory.CreateDirectory(GameConfig.OfflineChartsFolder);

        Directory.CreateDirectory(GameConfig.TempModsFolder);
        Directory.CreateDirectory(GameConfig.TempChartsFolder);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IGamePathDiscovery GamePaths { get; init; }

    #endregion Injections
}