using Avalonia.Platform.Storage;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed partial class LinuxLauncher : IPlatformLauncher
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

        Logger.LogInformation($"Open uri: {uri}");
        return Task.CompletedTask;
    }

    public async Task RevealFileAsync(string filePath)
    {
        if (await TryRevealFileAsync(filePath).ConfigureAwait(false))
        {
            Logger.LogInformation($"Reveal file: {filePath}");
            return;
        }

        var folderPath = Path.GetDirectoryName(filePath)!;
        await OpenFolderAsync(folderPath).ConfigureAwait(false);
    }

    #region Injections

    public required TopLevelProxy TopLevel { get; init; }
    public required ILogger<LinuxLauncher> Logger { get; init; }

    #endregion Injections
}
