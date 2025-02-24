namespace MuseDashModTools.Styles.ExtendControls;

public sealed class ButtonImage : Button
{
    public static readonly StyledProperty<IImage> ImageSourceProperty =
        AvaloniaProperty.Register<ButtonImage, IImage>(nameof(ImageSource));

    public static readonly StyledProperty<double> ImageHeightProperty =
        AvaloniaProperty.Register<ButtonImage, double>(nameof(ImageHeight));

    public static readonly StyledProperty<Thickness> ImageMarginProperty =
        AvaloniaProperty.Register<ButtonImage, Thickness>(nameof(ImageMargin));

    public static readonly StyledProperty<Cursor> ImageCursorProperty =
        AvaloniaProperty.Register<ButtonImage, Cursor>(nameof(ImageCursor));

    [Content]
    public IImage ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public double ImageHeight
    {
        get => GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public Thickness ImageMargin
    {
        get => GetValue(ImageMarginProperty);
        set => SetValue(ImageMarginProperty, value);
    }

    public Cursor ImageCursor
    {
        get => GetValue(ImageCursorProperty);
        set => SetValue(ImageCursorProperty, value);
    }
}