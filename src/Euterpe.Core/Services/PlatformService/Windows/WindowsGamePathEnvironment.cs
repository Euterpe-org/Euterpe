namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGamePathEnvironment : IGamePathEnvironment
{
    public bool IsSet()
    {
        var envValue = Environment.GetEnvironmentVariable("MD_DIRECTORY");
        return !envValue.IsNullOrEmpty() && envValue == GameConfig.Folder;
    }

    public bool Set()
    {
        try
        {
            Logger.ZLogInformation($"Set MD_DIRECTORY environment variable to: {GameConfig.Folder}");
            Environment.SetEnvironmentVariable("MD_DIRECTORY", GameConfig.Folder, EnvironmentVariableTarget.User);
            MessageBoxService.SuccessOverlayAsync(MessageBox_Content_SetPathEnvironment_Windows, GameConfig.Folder).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to set MD_DIRECTORY environment variable");
            return false;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsGamePathEnvironment> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}