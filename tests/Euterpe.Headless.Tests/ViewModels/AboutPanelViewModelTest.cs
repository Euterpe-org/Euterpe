using Euterpe.Abstractions;
using Euterpe.Features.Setting;

namespace Euterpe.Headless.Tests.ViewModels;

[TestSubject(typeof(AboutPanelViewModel))]
public sealed class AboutPanelViewModelTest : HeadlessTest
{
    [Test]
    public Task ContributorGroups_IsNotEmpty() => RunOnUI(async () =>
    {
        var vm = NewViewModel();
        await Assert.That(vm.ContributorGroups).IsNotEmpty();
    });

    [Test]
    public Task CheckUpdateCommand_NoUpdateFound_ShowsSuccessMessage() => RunOnUI(async () =>
    {
        var update = IUpdateService.Mock();
        update.CheckForUpdatesAsync(Any<CancellationToken>()).Returns(false);
        var msgBox = IMessageBoxService.Mock();
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        msgBox.SuccessAsync(Any<string>()).WasCalled(Times.Once);
    });

    [Test]
    public Task CheckUpdateCommand_UpdateFound_DoesNotShowSuccess() => RunOnUI(async () =>
    {
        var update = IUpdateService.Mock();
        update.CheckForUpdatesAsync(Any<CancellationToken>()).Returns(true);
        var msgBox = IMessageBoxService.Mock();
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        msgBox.SuccessAsync(Any<string>()).WasNeverCalled();
    });

    private static AboutPanelViewModel NewViewModel(
        IUpdateService? update = null,
        IMessageBoxService? msgBox = null) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        UpdateService = update ?? IUpdateService.Mock(),
        MessageBoxService = msgBox ?? IMessageBoxService.Mock()
    };
}
