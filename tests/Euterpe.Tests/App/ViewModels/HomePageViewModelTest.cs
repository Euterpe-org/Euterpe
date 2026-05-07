using Euterpe.ViewModels.Pages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("HomePageViewModelTests")]
[TestSubject(typeof(HomePageViewModel))]
public sealed class HomePageViewModelTest
{
    [Test]
    [Arguments(0, GameMode.Modded)]
    [Arguments(1, GameMode.Vanilla)]
    public async Task SelectedGameModeIndex_ChangedWritesToGameConfig(int index, GameMode expected)
    {
        var gameConfig = new MuseDashConfig();
        var vm = NewViewModel(gameConfig);

        vm.SelectedGameModeIndex = index;

        await Assert.That(gameConfig.GameMode).IsEqualTo(expected);
    }

    [Test]
    public async Task GameModes_HasModdedAndVanilla()
    {
        var vm = NewViewModel(new MuseDashConfig());

        await Assert.That(vm.GameModes).Count().IsEqualTo(2);
    }

    private static HomePageViewModel NewViewModel(GameConfig gameConfig) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<HomePageViewModel>.Instance,
        GameConfig = gameConfig,
        AccountClient = null!,
        GameLaunchService = IGameLaunchService.Mock(),
        GameSettingService = IGameSettingService.Mock(),
        GameLocalService = IGameLocalService.Mock(),
        MessageBoxService = IMessageBoxService.Mock(),
        RuntimeInstaller = IGameRuntimeInstaller.Mock(),
        UidProvider = IGameUidProvider.Mock(),
        NavigationService = null!,
        GameSwitcher = null!,
        WizardDialogViewModel = null!,
        RepairDialogViewModel = null!
    };
}