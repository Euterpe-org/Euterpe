namespace Euterpe.Core;

internal sealed class GameSettingService : IGameSettingService
{
    public async Task ValidateGameAsync()
    {
        Logger.ZLogInformation($"Checking for valid {GameConfig.DisplayName} setting...");

        await CheckGameFolderAsync().ConfigureAwait(true);
        CreateNecessaryFolders();

        Logger.ZLogInformation($"{GameConfig.DisplayName} setting validated");
    }

    private async Task CheckGameFolderAsync()
    {
        if (GameConfig.Folder.IsNullOrEmpty() || !GamePaths.CheckIsValidGameFolder(GameConfig.Folder))
        {
            Logger.ZLogError($"Stored {GameConfig.DisplayName} folder is invalid");

            var useDetectedPath = false;
            if (GamePaths.TryGetGameFolder(out var gameFolder))
            {
                var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_Confirm_DetectedMuseDashPath, gameFolder).ConfigureAwait(true);
                useDetectedPath = result is MessageBoxResult.Yes;
            }

            if (useDetectedPath)
            {
                GameConfig.Folder = gameFolder;
            }
            else
            {
                Logger.ZLogInformation($"Letting user choose {GameConfig.DisplayName} folder...");
                var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_ChooseMuseDashFolder).ConfigureAwait(true);
                if (result is not MessageBoxResult.Yes)
                {
                    Logger.ZLogInformation($"User cancelled {GameConfig.DisplayName} folder selection. Exiting application.");
                    Environment.Exit(0);
                }

                GameConfig.Folder = await GameLocalService.GetGameFolderAsync().ConfigureAwait(true);
            }
        }
    }

    private void CreateNecessaryFolders()
    {
        Directory.CreateDirectory(GameConfig.ModsFolder);
        Directory.CreateDirectory(GameConfig.UserLibsFolder);
        Directory.CreateDirectory(GameConfig.OnlineChartsFolder);
        Directory.CreateDirectory(GameConfig.OfflineChartsFolder);
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<GameSettingService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IGamePathDiscovery GamePaths { get; init; }

    #endregion Injections
}