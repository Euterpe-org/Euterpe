namespace Euterpe.Controls;

public sealed class Difficulty : TemplatedControl
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Difficulty, IImage?>(nameof(Source));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Difficulty, string?>(nameof(Text));

    [Content]
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}