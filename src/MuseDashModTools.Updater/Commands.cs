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
    /// <param name="pid"></param>
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

        foreach (var filePath in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(filePath);
            var destinationPath = Path.Combine(sourcePath, $"backup-{fromVersion}", fileName);
            File.Copy(filePath, destinationPath, true);
        }

        logger.ZLogInformation($"Backup completed at {sourcePath} for version {fromVersion}");

        localService.ExtractZipFile("MuseDashModTools.zip", sourcePath);
        logger.ZLogInformation($"Updated files extracted to {sourcePath}");

        Console.ReadKey();
    }
}