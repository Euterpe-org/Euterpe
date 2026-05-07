using Euterpe.ViewModels.Components;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

/// <summary>
///     Demonstrates ViewModel unit testing patterns:
///     instantiate VM directly, satisfy <c>required</c> dependencies (mock or null! the unused ones),
///     invoke commands, and assert side effects. No Avalonia or headless platform involved.
/// </summary>
[Category("RepairDialogViewModelTests")]
[TestSubject(typeof(RepairDialogViewModel))]
public sealed class RepairDialogViewModelTest
{
    [Test]
    public async Task ApplyCommand_RaisesRequestClose()
    {
        var vm = NewViewModel();
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.ApplyCommand.Execute(null);

        await Assert.That(closed).IsTrue();
    }

    [Test]
    public async Task Close_RaisesRequestClose()
    {
        var vm = NewViewModel();
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.Close();

        await Assert.That(closed).IsTrue();
    }

    [Test]
    public async Task Close_WithNoSubscriber_DoesNotThrow()
    {
        var vm = NewViewModel();

        vm.Close();

        await Task.CompletedTask;
    }

    [Test]
    public async Task OpenFileCommand_DelegatesToLauncher()
    {
        var launcher = IPlatformLauncher.Mock();
        var vm = NewViewModel(launcher);

        await vm.OpenFileCommand.ExecuteAsync("/path/to/file");

        launcher.OpenFileAsync("/path/to/file").WasCalled(Times.Once);
        await Task.CompletedTask;
    }

    [Test]
    public async Task OpenUrlCommand_DelegatesToLauncher()
    {
        var launcher = IPlatformLauncher.Mock();
        var vm = NewViewModel(launcher);

        await vm.OpenUrlCommand.ExecuteAsync("https://example.com");

        launcher.OpenUriAsync("https://example.com").WasCalled(Times.Once);
        await Task.CompletedTask;
    }

    private static RepairDialogViewModel NewViewModel(IPlatformLauncher? launcher = null) => new()
    {
        Launcher = launcher ?? IPlatformLauncher.Mock(),
        Logger = NullLogger<RepairDialogViewModel>.Instance,
        GameConfig = null!,
        ExecutionPage = null!,
        GamePathPage = null!,
        State = null!
    };
}