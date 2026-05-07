using System.Windows.Input;

namespace Euterpe.Headless.Tests.TestSupport;

internal sealed class DelegateCommand(Action<object?> action) : ICommand
{
    public DelegateCommand(Action action) : this(_ => action())
    {
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => action(parameter);
}