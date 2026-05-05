namespace Euterpe.Controls;

public sealed class RoleCard : TemplatedControl
{
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<RoleCard, Geometry?>(nameof(Icon));

    public static readonly StyledProperty<LocalizedString?> TitleProperty =
        AvaloniaProperty.Register<RoleCard, LocalizedString?>(nameof(Title));

    public static readonly StyledProperty<LocalizedString?> DescriptionProperty =
        AvaloniaProperty.Register<RoleCard, LocalizedString?>(nameof(Description));

    public static readonly StyledProperty<IBrush?> AccentColorProperty =
        AvaloniaProperty.Register<RoleCard, IBrush?>(nameof(AccentColor));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<RoleCard, bool>(nameof(IsSelected));

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public LocalizedString? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public LocalizedString? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IBrush? AccentColor
    {
        get => GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}