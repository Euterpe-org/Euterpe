using System.ComponentModel;

namespace Euterpe.Features.Charting;

public sealed partial class ChartFilterViewModel : ObservableObject
{
    private const int RatingLowerBound = 1;
    private const int RatingUpperBound = 12;

    private readonly Subject<string?> _propertyChanged = new();

    [ObservableProperty] public partial ChartSource Source { get; set; } = ChartSource.Online;
    [ObservableProperty] public partial string? SearchText { get; set; }

    [ObservableProperty] public partial bool ShowEasy { get; set; } = true;
    [ObservableProperty] public partial bool ShowHard { get; set; } = true;
    [ObservableProperty] public partial bool ShowMaster { get; set; } = true;
    [ObservableProperty] public partial bool ShowHidden { get; set; } = true;

    [ObservableProperty] public partial int? RatingMin { get; set; } = RatingLowerBound;
    [ObservableProperty] public partial int? RatingMax { get; set; } = RatingUpperBound;

    [ObservableProperty] public partial int? BpmMin { get; set; }
    [ObservableProperty] public partial int? BpmMax { get; set; }

    [ObservableProperty] public partial bool StreamerSafeOnly { get; set; }
    [ObservableProperty] public partial bool HasVideoOnly { get; set; }

    public Observable<Unit> Changed { get; }

    public ChartFilterViewModel() =>
        Changed = new[]
        {
            _propertyChanged.Where(static name => name != nameof(SearchText)),
            _propertyChanged.Where(static name => name == nameof(SearchText)).Debounce(AppConstants.SearchDebounce)
        }
        .Merge()
        .Select(static _ => Unit.Default);

    public void Reset()
    {
        SearchText = null;
        ShowEasy = ShowHard = ShowMaster = ShowHidden = true;
        RatingMin = RatingLowerBound;
        RatingMax = RatingUpperBound;
        BpmMin = null;
        BpmMax = null;
        StreamerSafeOnly = false;
        HasVideoOnly = false;
    }

    public bool Matches(ChartDto chart) =>
        MatchesSource(chart)
        && MatchesSearch(chart)
        && MatchesDifficulty(chart)
        && MatchesRating(chart)
        && MatchesBpm(chart)
        && MatchesStreamerSafe(chart)
        && MatchesVideo(chart);

    private bool MatchesSource(ChartDto chart) =>
        chart.Source == Source;

    private bool MatchesSearch(ChartDto chart)
    {
        if (SearchText.IsNullOrEmpty())
        {
            return true;
        }

        var meta = chart.Manifest.Meta;
        return meta.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || (meta.NameRomanized?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || meta.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || meta.Maps.Values.Any(m => m.Charters.Any(c => c.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
    }

    private bool MatchesDifficulty(ChartDto chart)
    {
        if (ShowEasy && ShowHard && ShowMaster && ShowHidden)
        {
            return true;
        }

        return (ShowEasy && chart.HasDifficulty(ChartDifficulty.Easy))
            || (ShowHard && chart.HasDifficulty(ChartDifficulty.Hard))
            || (ShowMaster && chart.HasDifficulty(ChartDifficulty.Master))
            || (ShowHidden && chart.HasDifficulty(ChartDifficulty.Hidden));
    }

    private bool MatchesRating(ChartDto chart)
    {
        var min = RatingMin ?? RatingLowerBound;
        var max = RatingMax ?? RatingUpperBound;
        if (min <= RatingLowerBound && max >= RatingUpperBound)
        {
            return true;
        }

        return chart.Manifest.Meta.Maps.Values.Any(m => (int)m.RatingValue >= min && (int)m.RatingValue <= max);
    }

    private bool MatchesBpm(ChartDto chart)
    {
        var bpm = chart.Manifest.Meta.Bpm;
        return (BpmMin is not { } min || bpm >= min) && (BpmMax is not { } max || bpm <= max);
    }

    private bool MatchesStreamerSafe(ChartDto chart) =>
        !StreamerSafeOnly || chart.Manifest.Meta.SafeForStreamer;

    private bool MatchesVideo(ChartDto chart) =>
        !HasVideoOnly || chart.HasVideo;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        _propertyChanged.OnNext(e.PropertyName);
    }
}
