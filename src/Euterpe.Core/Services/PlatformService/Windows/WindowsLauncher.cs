using Avalonia.Platform.Storage;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsLauncher : IPlatformLauncher
{
    public async Task OpenFileAsync(string filePath)
    {
        await TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath)).ConfigureAwait(false);
        Logger.LogInformation($"Open file: {filePath}");
    }

    public async Task OpenFolderAsync(string folderPath)
    {
        await TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath)).ConfigureAwait(false);
        Logger.LogInformation($"Open folder: {folderPath}");
    }

    public async Task OpenUriAsync(string uri)
    {
        await TopLevel.Launcher.LaunchUriAsync(new Uri(uri)).ConfigureAwait(false);
        Logger.LogInformation($"Open uri: {uri}");
    }

    public Task RevealFileAsync(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("explorer.exe", $"/select, {filePath}")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );
        Logger.LogInformation($"Reveal file: {filePath}");
        return Task.CompletedTask;
    }

    #region Injections

    public required TopLevelProxy TopLevel { get; init; }
    public required ILogger<WindowsLauncher> Logger { get; init; }

    #endregion Injections
}
