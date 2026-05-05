using System.Collections.ObjectModel;

namespace Euterpe.Controls.Models;

public sealed record DropDownButtonItem(LocalizedString Text, ObservableCollection<DropDownMenuItem>? MenuItems);