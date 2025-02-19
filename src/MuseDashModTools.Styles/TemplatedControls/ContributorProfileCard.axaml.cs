using MuseDashModTools.Models.Controls;

namespace MuseDashModTools.Styles.TemplatedControls;

public class ContributorProfileCard : TemplatedControl
{
    public static readonly StyledProperty<ContributorCardItem> SourceProperty =
        AvaloniaProperty.Register<ContributorProfileCard, ContributorCardItem>(nameof(Source));

    [Content]
    public ContributorCardItem Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
}