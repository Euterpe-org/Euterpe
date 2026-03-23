using System.Windows.Input;

namespace Euterpe.Styles.Models;

public sealed class DropDownMenuItem(string text, ICommand command, string? commandParameter = null)
{
    public LocalizedString Text { get; init; } = text;
    public ICommand Command { get; init; } = command;
    public string? CommandParameter { get; init; } = commandParameter;
}