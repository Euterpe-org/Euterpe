using Avalonia.Controls.Primitives;

namespace MuseDashModToolsUI.Views.Controls;

public class TitledTextBox : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleTextProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(TitleText));

    public static readonly StyledProperty<string?> ContentTextProperty =
        AvaloniaProperty.Register<TextBlock, string?>(nameof(ContentText));

    public string? ContentText
    {
        get => GetValue(ContentTextProperty);
        set => SetValue(ContentTextProperty, value);
    }

    public string? TitleText
    {
        get => GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }
}