namespace Euterpe.Services;

public sealed partial class GameSwitcher : ObservableObject
{
    [ObservableProperty]
    public partial bool CanSwitch { get; set; } = true;

    public async Task SwitchAsync(GameId target)
    {
        if (!CanSwitch || target == Config.ActiveGame)
        {
            return;
        }

        Logger.ZLogInformation($"Switching active game from {Config.ActiveGame} to {target}");

        Config.ActiveGame = target;
        await AppSettingService.SaveAsync().ConfigureAwait(false);
        IocContainer.ActivateGame(target);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IAppSettingService AppSettingService { get; init; }

    [UsedImplicitly]
    public required ILogger<GameSwitcher> Logger { get; init; }

    #endregion Injections
}