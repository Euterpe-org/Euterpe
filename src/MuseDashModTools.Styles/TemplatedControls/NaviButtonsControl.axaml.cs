using MuseDashModTools.Models;

namespace MuseDashModTools.Styles.TemplatedControls;

public class NaviButtonsControl : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable<DropDownButtonItem>> SourceProperty =
        AvaloniaProperty.Register<NaviButtonsControl, IEnumerable<DropDownButtonItem>>(nameof(Source));

    [Content]
    public IEnumerable<DropDownButtonItem> Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
}