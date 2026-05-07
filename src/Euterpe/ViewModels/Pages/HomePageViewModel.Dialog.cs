using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Pages;

public sealed partial class HomePageViewModel
{
    private async Task ShowFullWizardAsync()
    {
        await WizardDialogViewModel.PrepareForFullSetupAsync().ConfigureAwait(true);
        await ShowDialogAsync<WizardDialog, WizardDialogViewModel>(WizardDialogViewModel, Wizard_Title_Welcome).ConfigureAwait(true);
    }

    private async Task ShowOptionRepairAsync(SetupOptionKinds kind)
    {
        await RepairDialogViewModel.PrepareForOptionAsync(kind).ConfigureAwait(true);
        await ShowDialogAsync<RepairDialog, RepairDialogViewModel>(RepairDialogViewModel, Setup_Title_SettingUp).ConfigureAwait(true);
    }

    private async Task ShowGamePathRepairAsync()
    {
        await RepairDialogViewModel.PrepareForGamePathAsync().ConfigureAwait(true);
        await ShowDialogAsync<RepairDialog, RepairDialogViewModel>(RepairDialogViewModel, Setup_Title_GamePath).ConfigureAwait(true);
    }

    private async Task ShowDialogAsync<TView, TViewModel>(TViewModel vm, string title)
        where TView : Control, new()
        where TViewModel : ViewModelBase, IDialogContext
    {
        var options = new OverlayDialogOptions
        {
            Title = title,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        GameSwitcher.CanSwitch = false;
        try
        {
            await OverlayDialog.ShowCustomAsync<TView, TViewModel, object>(vm, MainWindowViewModel.DialogHostId, options).ConfigureAwait(true);
        }
        finally
        {
            GameSwitcher.CanSwitch = true;
        }
    }
}