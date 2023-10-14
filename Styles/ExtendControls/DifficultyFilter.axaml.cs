namespace MuseDashModToolsUI.Styles.ExtendControls;

public class DifficultyFilter : MenuItem
{
    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<DifficultyFilter, bool?>(nameof(IsChecked), false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        IsChecked = !IsChecked;
    }
}