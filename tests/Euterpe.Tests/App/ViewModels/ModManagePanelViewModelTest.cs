using Euterpe.ViewModels.Panels.Modding;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("ModManagePanelViewModelTests")]
[TestSubject(typeof(ModManagePanelViewModel))]
public sealed class ModManagePanelViewModelTest
{
    [Test]
    public async Task Constructor_InitializesWithEmptyMods()
    {
        var vm = NewViewModel();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.Mods).IsEmpty();
        await Assert.That(vm.SearchText).IsNull();
        await Assert.That(vm.SelectedModFilterIndex).IsEqualTo(0);
        await Assert.That(vm.AllModsLoaded).IsFalse();
    }

    [Test]
    public async Task ModFilterTypes_HasSixOptions() =>
        await Assert.That(ModManagePanelViewModel.ModFilterTypes).Count().IsEqualTo(6);

    [Test]
    public async Task SearchTextChanged_RaisesPropertyChanged()
    {
        var vm = NewViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.SearchText = "hello";

        await Assert.That(changed).Contains(nameof(ModManagePanelViewModel.SearchText));
    }

    [Test]
    public async Task OpenConfigFileCommand_DelegatesToLauncher_WithComposedPath()
    {
        var launcher = IPlatformLauncher.Mock();
        var openedFiles = new List<string>();
        launcher.OpenFileAsync(Any<string>()).Callback(p => openedFiles.Add(p));
        var gameConfig = new MuseDashConfig { Folder = "/games/musedash" };
        var vm = new ModManagePanelViewModel
        {
            Launcher = launcher,
            Logger = NullLogger<ModManagePanelViewModel>.Instance,
            Config = new Config { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() },
            GameConfig = gameConfig,
            ModManageService = IModManageService.Mock(),
            SelectedMod = new ModDto { Name = "TestMod", ConfigFile = "config.json" }
        };

        await vm.OpenConfigFileCommand.ExecuteAsync(null);

        using var assertions = Assert.Multiple();
        await Assert.That(openedFiles).Count().IsEqualTo(1);
        await Assert.That(openedFiles[0]).EndsWith("config.json");
        await Assert.That(openedFiles[0]).Contains(gameConfig.UserDataFolder);
    }

    private static ModManagePanelViewModel NewViewModel() => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<ModManagePanelViewModel>.Instance,
        Config = new Config { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() },
        GameConfig = new MuseDashConfig(),
        ModManageService = IModManageService.Mock()
    };
}