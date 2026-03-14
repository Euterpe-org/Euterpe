using System.Collections.ObjectModel;

namespace Euterpe.Styles.Models;

public sealed class DropDownButtonItem(string text, ObservableCollection<DropDownMenuItem>? menuItems)
{
    public LocalizedString Text { get; init; } = text;
    public ObservableCollection<DropDownMenuItem>? MenuItems { get; init; } = menuItems;
}