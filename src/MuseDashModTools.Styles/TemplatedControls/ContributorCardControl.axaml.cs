using MuseDashModTools.Models.Controls;

namespace MuseDashModTools.Styles.TemplatedControls;

public class ContributorCardControl : TemplatedControl
{
    public static readonly StyledProperty<ContributorCardItem> SourceProperty =
        AvaloniaProperty.Register<ContributorCardControl, ContributorCardItem>(nameof(Source));

    [Content]
    public ContributorCardItem Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
}