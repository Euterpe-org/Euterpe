namespace Euterpe.Core;

internal sealed class AppLocalService : IAppLocalService
{
    public async Task<string> GetSteamFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await EnsureValidSteamFolderAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseSteamFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected Steam folder: {path}");
        }

        return path;
    }

    public async Task<string> GetSteamExecPathAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await EnsureValidSteamExecPathAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFilePathAsync(FileDialog_Title_ChooseSteamExec).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected Steam executable: {path}");
        }

        return path;
    }

    public async Task<string> GetCacheFolderAsync()
    {
        var path = string.Empty;
        while (path.IsNullOrEmpty())
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseCacheFolder).ConfigureAwait(true);
            Logger.ZLogInformation($"Selected Cache folder: {path}");
        }

        return path;
    }

    private async ValueTask<bool> EnsureValidSteamFolderAsync(string folderPath)
    {
        if (SteamDiscovery.CheckIsValidSteamFolder(folderPath))
        {
            return true;
        }

        await MessageBoxService.ErrorAsync(MessageBox_Content_InvalidPath).ConfigureAwait(true);
        return false;
    }

    private async ValueTask<bool> EnsureValidSteamExecPathAsync(string execPath)
    {
        if (SteamDiscovery.CheckIsValidSteamExecPath(execPath))
        {
            return true;
        }

        await MessageBoxService.ErrorAsync(MessageBox_Content_InvalidPath).ConfigureAwait(true);
        return false;
    }

    #region Injections

    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required ILogger<AppLocalService> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required ISteamPathDiscovery SteamDiscovery { get; init; }

    #endregion Injections
}