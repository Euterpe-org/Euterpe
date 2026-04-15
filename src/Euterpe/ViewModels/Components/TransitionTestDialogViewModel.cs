using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Components;

public sealed class TransitionTestDialogViewModel : ViewModelBase, IDialogContext
{
    public void Close() => RequestClose?.Invoke(this, null);

    public event EventHandler<object?>? RequestClose;
}
