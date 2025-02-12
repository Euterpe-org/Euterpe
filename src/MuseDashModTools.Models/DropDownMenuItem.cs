using System.Windows.Input;

namespace MuseDashModTools.Models;

public sealed class DropDownMenuItem(string text, ICommand command)
{
    public string Text { get; init; } = text;
    public ICommand Command { get; init; } = command;
}