using System.Collections.ObjectModel;

namespace MuseDashModTools.Models.Controls;

public sealed class DropDownButtonItem(string text, ObservableCollection<DropDownMenuItem>? menuItems)
{
    public string Text { get; init; } = text;
    public ObservableCollection<DropDownMenuItem>? MenuItems { get; init; } = menuItems;
}