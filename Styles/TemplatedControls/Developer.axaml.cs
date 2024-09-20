namespace MuseDashModTools.Styles.TemplatedControls;

public sealed class Developer : TemplatedControl
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Image, IImage?>(nameof(Source));

    public static readonly StyledProperty<string?> DeveloperNameProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(DeveloperName));

    public static readonly StyledProperty<string?> ContributionTextProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(ContributionText));

    [Content]
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string? DeveloperName
    {
        get => GetValue(DeveloperNameProperty);
        set => SetValue(DeveloperNameProperty, value);
    }

    public string? ContributionText
    {
        get => GetValue(ContributionTextProperty);
        set => SetValue(ContributionTextProperty, value);
    }
}