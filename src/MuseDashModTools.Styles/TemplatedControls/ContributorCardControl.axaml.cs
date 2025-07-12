using System.Windows.Input;
using MuseDashModTools.Models.Controls;

namespace MuseDashModTools.Styles.TemplatedControls;

public sealed class ContributorCardControl : TemplatedControl
{
    public static readonly StyledProperty<IImage> AvatarProperty =
        AvaloniaProperty.Register<ContributorCardControl, IImage>(nameof(Avatar));

    public static readonly StyledProperty<string> ContributorNameProperty =
        AvaloniaProperty.Register<ContributorCardControl, string>(nameof(ContributorName));

    public static readonly StyledProperty<string?> ContributorDescriptionProperty =
        AvaloniaProperty.Register<ContributorCardControl, string?>(nameof(ContributorDescription));

    public static readonly StyledProperty<IEnumerable<ContributorLink>?> LinksProperty =
        AvaloniaProperty.Register<ContributorCardControl, IEnumerable<ContributorLink>?>(nameof(Links));

    public static readonly StyledProperty<ICommand> ButtonCommandProperty =
        AvaloniaProperty.Register<ContributorCardControl, ICommand>(nameof(ButtonCommand));

    [Content]
    public IImage Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    public string ContributorName
    {
        get => GetValue(ContributorNameProperty);
        set => SetValue(ContributorNameProperty, value);
    }

    public string? ContributorDescription
    {
        get => GetValue(ContributorDescriptionProperty);
        set => SetValue(ContributorDescriptionProperty, value);
    }

    public IEnumerable<ContributorLink>? Links
    {
        get => GetValue(LinksProperty);
        set => SetValue(LinksProperty, value);
    }

    public ICommand ButtonCommand
    {
        get => GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }
}