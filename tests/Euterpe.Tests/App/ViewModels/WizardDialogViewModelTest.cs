using Euterpe.ViewModels.Components;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("WizardDialogViewModelTests")]
[TestSubject(typeof(WizardDialogViewModel))]
public sealed class WizardDialogViewModelTest
{
    [Test]
    public async Task BackCommand_DecrementsCurrentPageIndex()
    {
        var vm = NewViewModel();
        vm.CurrentPageIndex = 2;

        vm.BackCommand.Execute(null);

        await Assert.That(vm.CurrentPageIndex).IsEqualTo(1);
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
    public async Task CurrentPageIndex_ChangeRaisesPropertyChangedForDerivedProperties()
    {
        var vm = NewViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.CurrentPageIndex = 1;

        using var _ = Assert.Multiple();
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.CurrentPageIndex));
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.CurrentPage));
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.CanGoBack));
        await Assert.That(changed).Contains(nameof(WizardDialogViewModel.IsLastPage));
    }

    private static WizardDialogViewModel NewViewModel() => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<WizardDialogViewModel>.Instance,
        GameConfig = null!,
        ExecutionPage = null!,
        GamePathPage = null!,
        RolePage = null!,
        State = null!
    };
}