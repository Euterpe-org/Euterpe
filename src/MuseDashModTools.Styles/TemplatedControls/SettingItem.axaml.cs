namespace MuseDashModTools.Styles.TemplatedControls;

public sealed class SettingItem : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingItem, string>(nameof(Title));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<SettingItem, string>(nameof(Description));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}