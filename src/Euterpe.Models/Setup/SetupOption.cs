namespace Euterpe.Models.Setup;

public sealed partial class SetupOption(SetupOptionKinds kinds, LocalizedString displayName, LocalizedString description) : ObservableObject
{
    public SetupOptionKinds Kinds { get; } = kinds;

    public LocalizedString DisplayName { get; } = displayName;

    public LocalizedString Description { get; } = description;

    public bool IsRequired { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}