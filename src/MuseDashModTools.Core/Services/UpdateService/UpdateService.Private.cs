namespace MuseDashModTools.Core;

internal sealed partial class UpdateService
{
    private async Task<bool> ShouldUpdateAsync(SemVersion releaseVersion)
    {
        if (Config.SkipVersion == releaseVersion)
        {
            Logger.ZLogInformation($"New version is skipped by user configuration");
            return false;
        }

        if (releaseVersion.ComparePrecedenceTo(_currentVersion) <= 0)
        {
            Logger.ZLogInformation($"No new version available");
            return false;
        }

        var result = await MessageBoxService.NoticeConfirmAsync($"New version available: {releaseVersion}, do you want to upgrade?")
            .ConfigureAwait(true);

        if (result is MessageBoxResult.Yes)
        {
            return true;
        }

        Logger.ZLogInformation($"User choose to skip this version: {releaseVersion}");
        Config.SkipVersion = releaseVersion;
        return false;
    }

    private static string GetUpdateTempPath()
    {
        var updateTempPath = Path.Combine(Path.GetTempPath(), AppName, "Update");
        Directory.CreateDirectory(updateTempPath);

        return updateTempPath;
    }

    private async Task StartUpdateProcessAsync(string version, CancellationToken cancellationToken = default)
    {
        var updateFolder = GetUpdateTempPath();
        var updaterTargetPath = Path.Combine(updateFolder, PlatformService.UpdaterFileName);

        await DownloadManager.DownloadReleaseByTagAsync(version, PlatformService.OsString, updateFolder, cancellationToken).ConfigureAwait(false);
        File.Copy(PlatformService.UpdaterFileName, updaterTargetPath, true);

        Process.Start(
            new ProcessStartInfo
            {
                FileName = updaterTargetPath,
                Arguments = $"update -d {AppDomain.CurrentDomain.BaseDirectory} -ov {AppVersion} -pid {Environment.ProcessId}",
                WorkingDirectory = updateFolder,
                UseShellExecute = false
            });
    }
}