namespace Euterpe.ViewModels.Components.Wizard;

public abstract class WizardPageViewModelBase : ViewModelBase
{
    public abstract LocalizedString Title { get; }

    public virtual bool CanGoNext => true;

    public virtual bool CanGoBack => true;

    public virtual LocalizedString NextButtonText => Button_Next;

    [UsedImplicitly]
    public required WizardState State { get; init; }

    public virtual Task OnEnterAsync() => Task.CompletedTask;
}