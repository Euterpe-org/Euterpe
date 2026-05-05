namespace Euterpe.Services;

public sealed class GameSwitcher
{
    public bool CanSwitch() => true;

    public async Task SwitchAsync(GameId target)
    {
        if (target == Config.ActiveGame)
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