using System.Collections.ObjectModel;

namespace MuseDashModTools.Models;

public sealed class DropDownButtonItem(string text, ObservableCollection<DropDownMenuItem>? items)
{
    public string Text { get; init; } = text;
    public ObservableCollection<DropDownMenuItem>? Items { get; init; } = items;
}