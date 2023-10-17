namespace MuseDashModToolsUI.Styles.ExtendControls;

public class TitledTextBox : TextBox
{
    public static readonly StyledProperty<string?> TitleTextProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(TitleText));

    public static readonly StyledProperty<Thickness> TitleMarginProperty =
        AvaloniaProperty.Register<TextBlock, Thickness>(nameof(TitleMargin));

    public static readonly StyledProperty<VerticalAlignment> TitleVerticalAlignmentProperty =
        AvaloniaProperty.Register<TextBlock, VerticalAlignment>(nameof(TitleVerticalAlignment));

    public string? TitleText
    {
        get => GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public Thickness TitleMargin
    {
        get => GetValue(TitleMarginProperty);
        set => SetValue(TitleMarginProperty, value);
    }

    public VerticalAlignment TitleVerticalAlignment
    {
        get => GetValue(TitleVerticalAlignmentProperty);
        set => SetValue(TitleVerticalAlignmentProperty, value);
    }
}