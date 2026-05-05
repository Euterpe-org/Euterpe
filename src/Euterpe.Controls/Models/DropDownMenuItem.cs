using System.Windows.Input;

namespace Euterpe.Controls.Models;

public sealed record DropDownMenuItem(LocalizedString Text, ICommand Command, string? CommandParameter = null);