using System.Collections.ObjectModel;

namespace MuseDashModTools.Models;

public sealed class DropDownButtonItem(string text, ObservableCollection<DropDownItem>? items)
{
    public string Text { get; init; } = text;
    public ObservableCollection<DropDownItem>? Items { get; init; } = items;
}