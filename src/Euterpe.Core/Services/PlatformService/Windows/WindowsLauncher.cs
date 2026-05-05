using Avalonia.Platform.Storage;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsLauncher : IPlatformLauncher
{
    public void RevealFile(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("explorer.exe", $"/select, {filePath}")
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

    public async Task OpenUriAsync(string uri)
    {
        await TopLevel.Launcher.LaunchUriAsync(new Uri(uri)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open uri: {uri}");
    }

    #region Injections

    [UsedImplicitly]
    public required TopLevelProxy TopLevel { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsLauncher> Logger { get; init; }

    #endregion Injections
}