namespace Euterpe.Core;

internal sealed partial class UpdateService
{
    private const int MaxRetries = 3;

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
        var updateFolder = Path.Combine(Path.GetTempPath(), AppName, "Update");
        Directory.CreateDirectory(updateFolder);

        return updateFolder;
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

        if (await TryStartUpdateProcessAsync(target).ConfigureAwait(false))
        {
            Environment.Exit(0);
        }

        return true;
    }

    private async Task<bool> TryStartUpdateProcessAsync(UpdateTarget target)
    {
        try
        {
            var updateFolder = GetUpdateTempPath();
            var zipPath = Path.Combine(updateFolder, $"{AppName}.zip");
            var updaterTargetPath = Path.Combine(updateFolder, PlatformInfo.UpdaterFileName);

            await DownloadUtils.DownloadVerifiedAsync(
                ct => AppDownloadManager.DownloadFileAsync(target.DownloadUrl, zipPath, cancellationToken: ct),
                zipPath, target.Sha256, $"Release {target.Version}", MaxRetries, Logger, CancellationToken.None).ConfigureAwait(true);

            Logger.ZLogInformation($"Release {target.Version} download finished");

            File.Copy(Path.Combine(AppContext.BaseDirectory, PlatformInfo.UpdaterFileName), updaterTargetPath, true);

            Logger.ZLogInformation($"Starting updater process");
            var updaterProcessInfo = new ProcessStartInfo
            {
                FileName = updaterTargetPath,
                ArgumentList =
                {
                    "update",
                    "-d",
                    AppContext.BaseDirectory,
                    "-zip",
                    zipPath,
                    "-ov",
                    AppVersion,
                    "-pid",
                    Environment.ProcessId.ToString()
                },
                WorkingDirectory = updateFolder,
                UseShellExecute = false
            };

            Process.Start(updaterProcessInfo);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to update to release {target.Version}");
            await MessageBoxService.ErrorAsync(MessageBox_Content_Update_Failed, target.Version).ConfigureAwait(false);
            return false;
        }
    }

    private sealed record UpdateTarget(SemVersion Version, string DownloadUrl, string Sha256);
}
