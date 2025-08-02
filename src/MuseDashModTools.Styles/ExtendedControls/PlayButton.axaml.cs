using System.Windows.Input;

namespace MuseDashModTools.Styles.ExtendedControls;

public sealed class PlayButton : SelectingItemsControl
{
    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<PlayButton, ICommand>(nameof(Command));

    public static readonly StyledProperty<string> ContentProperty =
        AvaloniaProperty.Register<PlayButton, string>(nameof(Content));

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    [Content]
    public string Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}