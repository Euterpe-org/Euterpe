using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.Models;

public sealed partial class PageNavItem(string displayName, string key) : ObservableObject
{
    public PageNavItem[] Children { get; init; } = [];
    public string DisplayName { get; set; } = displayName;
    public string Key { get; set; } = key;
    public string? Status { get; init; }
    public bool IsNavigable { get; init; } = true;
    public bool IsSeparator { get; init; }

    [RelayCommand]
    private void Navigation()
    {
        if (!IsNavigable || IsSeparator)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(Key, "NavigatePage");
    }
}