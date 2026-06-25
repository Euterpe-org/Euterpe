namespace Euterpe.Updater;

public sealed class Commands
{
    /// <summary>
    ///     Applies a downloaded update and relaunches the application.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="localService"></param>
    /// <param name="platformInfo"></param>
    /// <param name="sourceDirectory">-d, Source directory where the application is.</param>
    /// <param name="oldVersion">-ov, Current version of the application.</param>
    /// <param name="pid">-pid, Process ID.</param>
    [Command("update")]
    public async Task UpdateAsync(
        [FromServices] ILogger<Commands> logger,
        [FromServices] ILocalService localService,
        [FromServices] IPlatformInfo platformInfo,
        string sourceDirectory,
        string oldVersion,
        int pid)
    {
        try
        {
            using var mainProcess = Process.GetProcessById(pid);
            await mainProcess.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            logger.ZLogInformation($"Process with ID {pid} has exited successfully.");
        }
        catch (ArgumentException)
        {
            logger.ZLogInformation($"Process with ID {pid} has already exited.");
        }

        try
        {
            var backupDirectory = Path.Combine(sourceDirectory, $"backup-{oldVersion}");
            Directory.CreateDirectory(backupDirectory);
            logger.ZLogInformation($"Backup folder created at {backupDirectory}");

            localService.CopyDirectory(sourceDirectory, backupDirectory);
            logger.ZLogInformation($"Backup completed for version {oldVersion}");

            localService.ExtractZipFile("Euterpe.zip", sourceDirectory);
            logger.ZLogInformation($"Update completed successfully!");

            var applicationPath = Path.Combine(sourceDirectory, platformInfo.ApplicationFileName);
            Process.Start(
                new ProcessStartInfo(applicationPath)
                {
                    WorkingDirectory = sourceDirectory,
                    UseShellExecute = false
                });

            logger.ZLogInformation($"Launched application: {applicationPath}");
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Update failed. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
