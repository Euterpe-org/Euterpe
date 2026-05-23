using System.Runtime.CompilerServices;
using Euterpe.Abstractions;
using Euterpe.Features.Setting;

namespace Euterpe.Headless.Tests.ViewModels;

/// <summary>
///     Lives here (not in Euterpe.Tests) because <c>AboutPanelViewModel.ContributorGroups</c>
///     constructs <c>Contributor</c> objects whose ctor calls <c>AssetLoader.Open(avares://...)</c>,
///     which needs the Avalonia platform initialized — exactly what the headless test session
///     gives us via <c>HeadlessTest</c>.
/// </summary>
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
        var successCalls = new StrongBox<int>(0);
        msgBox.SuccessAsync(Any<string>()).Callback(() => successCalls.Value++);
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        await Assert.That(successCalls.Value).IsEqualTo(1);
    });

    [Test]
    public Task CheckUpdateCommand_UpdateFound_DoesNotShowSuccess() => RunOnUI(async () =>
    {
        var update = IUpdateService.Mock();
        update.CheckForUpdatesAsync(Any<CancellationToken>()).Returns(true);
        var msgBox = IMessageBoxService.Mock();
        var successCalls = new StrongBox<int>(0);
        msgBox.SuccessAsync(Any<string>()).Callback(() => successCalls.Value++);
        var vm = NewViewModel(update, msgBox);

        await vm.CheckUpdateCommand.ExecuteAsync(null);

        await Assert.That(successCalls.Value).IsEqualTo(0);
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