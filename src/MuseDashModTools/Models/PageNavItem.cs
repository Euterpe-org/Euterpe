using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.Models;

public sealed class PageNavItem(string name)
{
    public PageNavItem[] Children { get; set; } = [];
    public string Name { get; set; } = name;
    public bool IsSeparator { get; set; }
    public bool IsNavigable { get; init; } = true;
    public StreamGeometry? Icon { get; set; }

    public ICommand NavigationCommand => new RelayCommand(() =>
    {
        if (IsSeparator || !IsNavigable)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(Name);
    });

    public static implicit operator PageNavItem(string name) => new(name);
}