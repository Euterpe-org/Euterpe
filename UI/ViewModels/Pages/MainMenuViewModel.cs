#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.ViewModels.Pages;

public sealed partial class MainMenuViewModel : ViewModelBase, IMainMenuViewModel
{
    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [RelayCommand]
    private void OnLaunchVanillaGame() => LocalService.OnLaunchGame(false);

    [RelayCommand]
    private void OnLaunchModdedGame() => LocalService.OnLaunchGame(true);
}