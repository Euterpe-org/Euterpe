using Avalonia.Media;

namespace Euterpe.ViewModels.Components;

public sealed partial class WizardIdentityOption(WizardIdentity identity, string iconKey, string title, string description, string accentColor) : ObservableObject
{
    public WizardIdentity Identity { get; } = identity;

    public string IconKey { get; } = iconKey;

    public string Title { get; } = title;

    public string Description { get; } = description;

    public string AccentColor { get; } = accentColor;

    private IBrush AccentBrush { get; } = SolidColorBrush.Parse(accentColor);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    public partial bool IsSelected { get; set; }

    public IBrush BorderBrush => IsSelected ? Brushes.White : Brushes.Transparent;
}
