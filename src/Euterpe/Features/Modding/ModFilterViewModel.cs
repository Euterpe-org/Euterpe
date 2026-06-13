using System.ComponentModel;

namespace Euterpe.Features.Modding;

public sealed partial class ModFilterViewModel : ObservableObject
{
    private readonly Subject<string?> _propertyChanged = new();

    [ObservableProperty] public partial string? SearchText { get; set; }
    [ObservableProperty] public partial ModFilterType ModFilter { get; set; } = ModFilterType.All;

    public Observable<Unit> Changed { get; }

    public ModFilterViewModel() =>
        Changed = new[]
        {
            _propertyChanged.Where(static name => name != nameof(SearchText)),
            _propertyChanged.Where(static name => name == nameof(SearchText)).Debounce(AppConstants.SearchDebounce)
        }
        .Merge()
        .Select(static _ => Unit.Default);

    public bool Matches(ModDto mod)
    {
        if (!SearchText.IsNullOrEmpty()
            && !mod.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            && !mod.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ModFilter switch
        {
            ModFilterType.Installed => mod.IsLocal,
            ModFilterType.Enabled => mod is { IsDisabled: false, IsLocal: true },
            ModFilterType.Disabled => mod is { IsDisabled: true, IsLocal: true },
            ModFilterType.Outdated => mod.State is ModState.Outdated,
            ModFilterType.Incompatible => mod is { State: ModState.Incompatible, IsLocal: true },
            _ => true
        };
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        _propertyChanged.OnNext(e.PropertyName);
    }
}
