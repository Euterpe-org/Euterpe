namespace Euterpe.ViewModels.Pages;

public sealed partial class HomePageViewModel
{
    private async Task ShowFullWizardAsync()
    {
        await WizardDialogViewModel.PrepareForFullSetupAsync().ConfigureAwait(true);
        await ShowWizardDialogAsync(Wizard_Title_Welcome).ConfigureAwait(true);
    }

    private async Task ShowOptionWizardAsync(WizardOptionKinds kind)
    {
        await WizardDialogViewModel.PrepareForOptionAsync(kind).ConfigureAwait(true);
        await ShowWizardDialogAsync(Wizard_Title_SettingUp).ConfigureAwait(true);
    }

    private async Task ShowGamePathWizardAsync()
    {
        await WizardDialogViewModel.PrepareForGamePathAsync().ConfigureAwait(true);
        await ShowWizardDialogAsync(Wizard_Title_GamePath).ConfigureAwait(true);
    }

    private async Task ShowWizardDialogAsync(string title)
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
            await OverlayDialog.ShowCustomAsync<WizardDialog, WizardDialogViewModel, object>(WizardDialogViewModel, MainWindowViewModel.WizardHostId, options).ConfigureAwait(true);
        }
        finally
        {
            GameSwitcher.CanSwitch = true;
        }
    }
}