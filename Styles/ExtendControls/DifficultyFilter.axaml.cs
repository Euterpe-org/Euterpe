namespace MuseDashModToolsUI.Styles.ExtendControls;

public class DifficultyFilter : MenuItem
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        IsChecked = !IsChecked;
    }
}