using System.Windows.Input;

namespace Euterpe.Styles.Models;

public sealed record DropDownMenuItem(LocalizedString Text, ICommand Command, string? CommandParameter = null);