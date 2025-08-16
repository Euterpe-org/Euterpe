using System.Diagnostics;

namespace MuseDashModTools.Updater;

public sealed class Commands
{
    /// <summary>
    ///     From which version to create a backup.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="localService"></param>
    /// <param name="sourceDirectory">-d, Source directory where the application is.</param>
    /// <param name="oldVersion">-ov, Current version of the application.</param>
    /// <param name="pid">-pid, Process ID.</param>
    [Command("update")]
    public async Task UpdateAsync(
        [FromServices] ILogger<Commands> logger,
        [FromServices] ILocalService localService,
        string sourceDirectory,
        string oldVersion,
        int pid)
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

        Directory.CreateDirectory(Path.Combine(sourceDirectory, $"backup-{oldVersion}"));
        logger.ZLogInformation($"Backup folder created at {sourceDirectory} for version {oldVersion}");

        localService.CopyDirectory(sourceDirectory, Path.Combine(sourceDirectory, $"backup-{oldVersion}"));
        logger.ZLogInformation($"Backup completed for version {oldVersion}");

        if (!localService.ExtractZipFile("MuseDashModTools.zip", sourceDirectory))
        {
            return;
        }

        logger.ZLogInformation($"Updated files extracted to {sourceDirectory}");
        logger.ZLogInformation($"Update completed successfully!");

        Console.ReadKey();
    }
}