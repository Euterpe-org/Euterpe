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
    /// <param name="zipPath">-zip, Absolute path to the downloaded update package.</param>
    /// <param name="oldVersion">-ov, Current version of the application.</param>
    /// <param name="pid">-pid, Process ID.</param>
    [Command("update")]
    public async Task UpdateAsync(
        [FromServices] ILogger<Commands> logger,
        [FromServices] ILocalService localService,
        [FromServices] IPlatformInfo platformInfo,
        string sourceDirectory,
        string zipPath,
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
        catch (TimeoutException)
        {
            logger.ZLogError($"Process with ID {pid} is still running after 30 seconds, aborting update. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        if (!localService.IsReadableZipFile(zipPath))
        {
            logger.ZLogError($"Update package at {zipPath} is missing or unreadable, aborting update. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        var backupDirectory = Path.Combine(sourceDirectory, $"backup-{oldVersion}");
        try
        {
            Directory.CreateDirectory(backupDirectory);
            logger.ZLogInformation($"Backup folder created at {backupDirectory}");

            localService.CopyDirectory(sourceDirectory, backupDirectory);
            logger.ZLogInformation($"Backup completed for version {oldVersion}");

            localService.ExtractZipFile(zipPath, sourceDirectory);
            logger.ZLogInformation($"Update completed successfully!");
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex,
                $"Update failed and the installation may be incomplete. Your previous version is backed up at {backupDirectory}. Restore it manually or reinstall. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        localService.TryDeleteFile(zipPath);

        try
        {
            LaunchApplication(logger, sourceDirectory, platformInfo.ApplicationFileName);
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Update completed, but the application could not be started automatically. Please start it manually. Press any key to exit.");
            Console.ReadKey();
        }
    }

    private static void LaunchApplication(ILogger<Commands> logger, string sourceDirectory, string applicationFileName)
    {
        var applicationPath = Path.Combine(sourceDirectory, applicationFileName);
        Process.Start(
            new ProcessStartInfo(applicationPath)
            {
                WorkingDirectory = sourceDirectory,
                UseShellExecute = false
            });

        logger.ZLogInformation($"Launched application: {applicationPath}");
    }
}
