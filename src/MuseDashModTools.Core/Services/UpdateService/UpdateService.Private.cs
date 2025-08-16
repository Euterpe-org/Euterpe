namespace MuseDashModTools.Core;

internal sealed partial class UpdateService
{
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
                Arguments = $"update {AppDomain.CurrentDomain.BaseDirectory} {AppVersion} {Environment.ProcessId}",
                WorkingDirectory = updateFolder,
                UseShellExecute = false
            });
    }
}