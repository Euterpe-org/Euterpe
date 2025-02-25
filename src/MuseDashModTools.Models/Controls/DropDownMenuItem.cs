using System.Windows.Input;

namespace MuseDashModTools.Models.Controls;

public sealed class DropDownMenuItem(string text, ICommand command, string? commandParameter = null)
{
    public string Text { get; init; } = text;
    public ICommand Command { get; init; } = command;
    public string? CommandParameter { get; init; } = commandParameter;
}