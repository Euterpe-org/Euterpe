using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace MuseDashModToolsUI.Styles.ExtendControls;

public class TitledTextBox : TemplatedControl
{
    // Text Block
    public static readonly StyledProperty<string?> TitleTextProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(TitleText));

    public static readonly StyledProperty<Thickness> TitleMarginProperty =
        AvaloniaProperty.Register<TextBlock, Thickness>(nameof(TitleMargin));

    public static readonly StyledProperty<VerticalAlignment> TitleVerticalAlignmentProperty =
        AvaloniaProperty.Register<TextBlock, VerticalAlignment>(nameof(TitleVerticalAlignment));

    // Text Box
    public static readonly StyledProperty<string?> ContentTextProperty =
        AvaloniaProperty.Register<TextBox, string?>(nameof(ContentText));

    public static readonly StyledProperty<VerticalAlignment> ContentVerticalAlignmentProperty =
        AvaloniaProperty.Register<TextBox, VerticalAlignment>(nameof(ContentVerticalAlignment));

    public static readonly StyledProperty<double> ContentMinWidthProperty =
        AvaloniaProperty.Register<TextBox, double>(nameof(ContentMinWidth));

    // Text Block
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

    // Text Box
    public string? ContentText
    {
        get => GetValue(ContentTextProperty);
        set => SetValue(ContentTextProperty, value);
    }

    public VerticalAlignment ContentVerticalAlignment
    {
        get => GetValue(ContentVerticalAlignmentProperty);
        set => SetValue(ContentVerticalAlignmentProperty, value);
    }

    public double ContentMinWidth
    {
        get => GetValue(ContentMinWidthProperty);
        set => SetValue(ContentMinWidthProperty, value);
    }
}