namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGamePathEnvironment : IGamePathEnvironment
{
    public bool IsSet()
    {
        var envValue = Environment.GetEnvironmentVariable(GameConfig.PathEnvironmentVariableName);
        return !envValue.IsNullOrEmpty() && envValue == GameConfig.Folder;
    }

    public bool Set()
    {
        try
        {
            Logger.ZLogInformation($"Set {GameConfig.PathEnvironmentVariableName} environment variable to: {GameConfig.Folder}");
            Environment.SetEnvironmentVariable(GameConfig.PathEnvironmentVariableName, GameConfig.Folder, EnvironmentVariableTarget.User);
            MessageBoxService.SuccessOverlayAsync(MessageBox_Content_SetPathEnvironment_Windows, GameConfig.Folder).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to set {GameConfig.PathEnvironmentVariableName} environment variable");
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