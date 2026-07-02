using Avalonia.Platform.Storage;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxLauncher : IPlatformLauncher
{
    public void RevealFile(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("xdg-open", filePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );

        Logger.ZLogInformation($"Reveal file: {filePath}");
    }

    public async Task OpenFolderAsync(string folderPath)
    {
        await TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open folder: {folderPath}");
    }

    public async Task OpenFileAsync(string filePath)
    {
        await TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open file: {filePath}");
    }

    public Task OpenUriAsync(string uri)
    {
        // Avalonia's Linux launcher corrupts uri query strings via broken shell escaping (AvaloniaUI/Avalonia#20230)
        Process.Start(
            new ProcessStartInfo("xdg-open")
            {
                ArgumentList = { uri },
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );

        Logger.ZLogInformation($"Open uri: {uri}");
        return Task.CompletedTask;
    }

    #region Injections

    public required TopLevelProxy TopLevel { get; init; }
    public required ILogger<LinuxLauncher> Logger { get; init; }

    #endregion Injections
}
