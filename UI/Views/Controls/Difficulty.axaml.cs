using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace MuseDashModToolsUI.Views.Controls;

public class Difficulty : TemplatedControl
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Image, IImage?>(nameof(Source));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(Text));

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