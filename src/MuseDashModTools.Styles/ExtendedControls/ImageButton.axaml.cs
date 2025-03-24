namespace MuseDashModTools.Styles.ExtendedControls;

public sealed class ImageButton : Button
{
    public static readonly StyledProperty<IImage> ImageSourceProperty =
        AvaloniaProperty.Register<ImageButton, IImage>(nameof(ImageSource));

    public static readonly StyledProperty<double> ImageHeightProperty =
        AvaloniaProperty.Register<ImageButton, double>(nameof(ImageHeight));

    public static readonly StyledProperty<Thickness> ImageMarginProperty =
        AvaloniaProperty.Register<ImageButton, Thickness>(nameof(ImageMargin));

    public static readonly StyledProperty<Cursor?> ImageCursorProperty =
        AvaloniaProperty.Register<ImageButton, Cursor?>(nameof(ImageCursor));

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

    public Cursor? ImageCursor
    {
        get => GetValue(ImageCursorProperty);
        set => SetValue(ImageCursorProperty, value);
    }
}