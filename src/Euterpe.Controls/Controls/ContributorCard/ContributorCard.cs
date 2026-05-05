using System.Windows.Input;
using Euterpe.Controls.Models;

namespace Euterpe.Controls;

public sealed class ContributorCard : TemplatedControl
{
    public static readonly StyledProperty<IImage> AvatarProperty =
        AvaloniaProperty.Register<ContributorCard, IImage>(nameof(Avatar));

    public static readonly StyledProperty<string> ContributorNameProperty =
        AvaloniaProperty.Register<ContributorCard, string>(nameof(ContributorName));

    public static readonly StyledProperty<string?> ContributorDescriptionProperty =
        AvaloniaProperty.Register<ContributorCard, string?>(nameof(ContributorDescription));

    public static readonly StyledProperty<IEnumerable<ContributorLink>?> LinksProperty =
        AvaloniaProperty.Register<ContributorCard, IEnumerable<ContributorLink>?>(nameof(Links));

    public static readonly StyledProperty<ICommand> ButtonCommandProperty =
        AvaloniaProperty.Register<ContributorCard, ICommand>(nameof(ButtonCommand));

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