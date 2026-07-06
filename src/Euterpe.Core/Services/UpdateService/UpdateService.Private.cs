namespace Euterpe.Core;

internal sealed partial class UpdateService
{
    private async Task<bool> ShouldUpdateAsync(SemVersion releaseVersion)
    {
        if (releaseVersion.ComparePrecedenceTo(CurrentVersion) <= 0)
        {
            Logger.ZLogInformation($"No new version available");
            return false;
        }

        await MessageBoxService.NoticeAsync(MessageBox_Content_NewVersionAvailable, releaseVersion).ConfigureAwait(true);
        return true;
    }

    private static string GetUpdateTempPath()
    {
        var updateTempPath = Path.Combine(Path.GetTempPath(), AppName, "Update");
        Directory.CreateDirectory(updateTempPath);

        return updateTempPath;
    }

    private async Task<bool> HandleReleaseAsync(UpdateTarget? target)
    {
        if (target is null)
        {
            return false;
        }

        Logger.ZLogInformation($"Release version parsed: {target.Version}");

        var shouldUpdate = await ShouldUpdateAsync(target.Version).ConfigureAwait(false);
        if (!shouldUpdate)
        {
            return false;
        }

        await StartUpdateProcessAsync(target).ConfigureAwait(false);
        Environment.Exit(0);
        return true;
    }

    private async Task StartUpdateProcessAsync(UpdateTarget target)
    {
        var updateFolder = GetUpdateTempPath();
        var updaterTargetPath = Path.Combine(updateFolder, PlatformInfo.UpdaterFileName);

        try
        {
            await AppDownloadManager.DownloadReleaseAsync(target.DownloadUrl, updateFolder).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download release {target.Version}");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ReleaseDownload_Failed, target.Version).ConfigureAwait(true);
            return;
        }

        Logger.ZLogInformation($"Release {target.Version} download finished");

        File.Copy(PlatformInfo.UpdaterFileName, updaterTargetPath, true);

        Logger.ZLogInformation($"Starting updater process");

        var updaterProcessInfo = new ProcessStartInfo
        {
            FileName = updaterTargetPath,
            ArgumentList =
            {
                "update",
                "-d",
                AppDomain.CurrentDomain.BaseDirectory,
                "-ov",
                AppVersion,
                "-pid",
                Environment.ProcessId.ToString()
            },
            WorkingDirectory = updateFolder,
            UseShellExecute = false
        };

        Process.Start(updaterProcessInfo);
    }

    private sealed record UpdateTarget(SemVersion Version, string DownloadUrl);
}
