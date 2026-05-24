using Ursa.Controls;

namespace Euterpe.Core;

internal sealed partial class UpdateService
{
    private async Task<bool> ShouldUpdateAsync(SemVersion releaseVersion)
    {
        if (Config.SkipVersion == releaseVersion)
        {
            Logger.ZLogInformation($"New version is skipped by user configuration");
            return false;
        }

        if (releaseVersion.ComparePrecedenceTo(CurrentVersion) <= 0)
        {
            Logger.ZLogInformation($"No new version available");
            return false;
        }

        var result = await MessageBoxService.NoticeConfirmAsync(MessageBox_Content_NewVersionAvailable, releaseVersion).ConfigureAwait(true);
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

    private async Task<bool> HandleReleaseAsync(UpdateTarget? target, CancellationToken cancellationToken = default)
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

        await StartUpdateProcessAsync(target, cancellationToken).ConfigureAwait(false);
        Environment.Exit(0);
        return true;
    }

    private async Task StartUpdateProcessAsync(UpdateTarget target, CancellationToken cancellationToken = default)
    {
        var updateFolder = GetUpdateTempPath();
        var updaterTargetPath = Path.Combine(updateFolder, PlatformInfo.UpdaterFileName);

        try
        {
            await AppDownloadManager.DownloadReleaseAsync(target.DownloadUrl, updateFolder, cancellationToken).ConfigureAwait(true);
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
        Process.Start(
            new ProcessStartInfo
            {
                FileName = updaterTargetPath,
                Arguments = $"update -d {AppDomain.CurrentDomain.BaseDirectory} -ov {AppVersion} -pid {Environment.ProcessId}",
                WorkingDirectory = updateFolder,
                UseShellExecute = false
            });
    }

    private sealed record UpdateTarget(SemVersion Version, string DownloadUrl);
}