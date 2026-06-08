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

        // Clean up temp folders on startup
        FileSystemService.TryDeleteDirectory(GameConfig.TempFolder, DeleteOption.IgnoreIfNotFound);
        Directory.CreateDirectory(GameConfig.TempModsFolder);
        Directory.CreateDirectory(GameConfig.TempChartsFolder);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IGamePathDiscovery GamePaths { get; init; }
    public required IFileSystemService FileSystemService { get; init; }

    #endregion Injections
}