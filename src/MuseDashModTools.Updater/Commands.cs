using System.Diagnostics;

namespace MuseDashModTools.Updater;

public sealed class Commands
{
    /// <summary>
    ///     From which version to create a backup.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="localService"></param>
    /// <param name="sourcePath">-p, Source path.</param>
    /// <param name="fromVersion">-fv, From which version.</param>
    /// <param name="pid">-pid, Process ID.</param>
    [Command("update")]
    public async Task UpdateAsync(
        [FromServices] ILogger<Commands> logger,
        [FromServices] ILocalService localService,
        [Argument] string sourcePath,
        [Argument] string fromVersion,
        [Argument] int pid)
    {
        try
        {
            var mainProcess = Process.GetProcessById(pid);
            await mainProcess.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            logger.ZLogInformation($"Process with ID {pid} has exited successfully.");
        }
        catch (ArgumentException)
        {
            logger.ZLogInformation($"Process with ID {pid} has already exited.");
        }

        Directory.CreateDirectory(Path.Combine(sourcePath, $"backup-{fromVersion}"));
        logger.ZLogInformation($"Backup folder created at {sourcePath} for version {fromVersion}");

        localService.CopyDirectory(sourcePath, Path.Combine(sourcePath, $"backup-{fromVersion}"));
        logger.ZLogInformation($"Backup completed for version {fromVersion}");

        if (!localService.ExtractZipFile("MuseDashModTools.zip", sourcePath))
        {
            return;
        }

        logger.ZLogInformation($"Updated files extracted to {sourcePath}");
        logger.ZLogInformation($"Update completed successfully!");

        Console.ReadKey();
    }
}