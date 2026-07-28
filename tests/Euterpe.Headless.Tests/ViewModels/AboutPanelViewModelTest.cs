using Euterpe.Abstractions;
using Euterpe.Features.Setting;
using Euterpe.Features.Update;
using Euterpe.Services;
using Ursa.Controls;

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
        update.CheckForUpdatesAsync().Returns((string?)null);
        var msgBox = IMessageBoxService.Mock();
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        msgBox.SuccessAsync(Any<string>()).WasCalled(Times.Once);
    });

    [Test]
    public Task CheckUpdateCommand_UpdateFound_DoesNotShowSuccess() => RunOnUI(async () =>
    {
        var update = IUpdateService.Mock();
        update.CheckForUpdatesAsync().Returns("2.1.0");
        var msgBox = IMessageBoxService.Mock();
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        msgBox.SuccessAsync(Any<string>()).WasNeverCalled();
        update.UpdateAsync(Any<IProgress<int>>()).WasCalled(Times.Once);
    });

    private static AboutPanelViewModel NewViewModel(
        IUpdateService? update = null,
        IMessageBoxService? msgBox = null)
    {
        var updateService = update ?? IUpdateService.Mock();
        var dialogService = IDialogService.Mock();
        dialogService.ShowOverlayAsync<UpdateDialog, UpdateDialogViewModel>(
            Any<UpdateDialogViewModel>(), Any<OverlayDialogOptions?>(), Any<string?>(), Any<CancellationToken?>());

        return new AboutPanelViewModel
        {
            Launcher = IPlatformLauncher.Mock(),
            Logger = Mock.Logger<AboutPanelViewModel>(),
            MessageBoxService = msgBox ?? IMessageBoxService.Mock(),
            UpdateDialogService = new UpdateDialogService
            {
                DialogService = dialogService,
                Logger = Mock.Logger<UpdateDialogService>(),
                MessageBoxService = msgBox ?? IMessageBoxService.Mock(),
                UpdateService = updateService,
                UpdateDialogViewModelFactory = static version => new UpdateDialogViewModel(version)
            },
            UpdateService = updateService
        };
    }
}
