using System.Collections.ObjectModel;

namespace Euterpe.Styles.Models;

public sealed record DropDownButtonItem(LocalizedString Text, ObservableCollection<DropDownMenuItem>? MenuItems);