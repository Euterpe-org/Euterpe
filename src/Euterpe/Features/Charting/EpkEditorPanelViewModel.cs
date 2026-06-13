using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Euterpe.Features.Charting;

[Register]
public sealed partial class EpkEditorPanelViewModel : ViewModelBase
{
    private bool _loading;
    private bool _dirty;
    private string? _filePath;
    private string? _folder;
    private Manifest? _original;
    private Dictionary<string, ManifestFileEntry> _files = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? NameRomanized { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string Author { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial bool SafeForStreamer { get; set; }

    [ObservableProperty]
    public partial int? Bpm { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsBpmRange { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial int? BpmMin { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial int? BpmMax { get; set; }

    [ObservableProperty]
    public partial string Scene { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SceneEgg { get; set; }

    [ObservableProperty]
    public partial float? BackgroundVideoOpacity { get; set; }

    [ObservableProperty]
    public partial string SearchKeywordsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasHiddenDifficulty { get; set; }

    [ObservableProperty]
    public partial string? HideMode { get; set; }

    [ObservableProperty]
    public partial string? HideRatingOverride { get; set; }

    [ObservableProperty]
    public partial string? HideMessage { get; set; }

    public ObservableCollection<DifficultyEditViewModel> Maps { get; } = [];

    public ObservableCollection<EpkFileEntry> Files { get; } = [];

    public event Action? CloseRequested;

    public void Open(string filePath, Manifest manifest)
    {
        _loading = true;

        _filePath = filePath;
        _folder = Path.GetDirectoryName(filePath);
        _original = manifest;
        _files = new Dictionary<string, ManifestFileEntry>(manifest.Files, StringComparer.OrdinalIgnoreCase);
        var meta = manifest.Meta;

        Name = meta.Name;
        NameRomanized = meta.NameRomanized;
        Author = meta.Author;
        Description = meta.Description;
        SafeForStreamer = meta.SafeForStreamer;
        Bpm = meta.Bpm;
        IsBpmRange = meta is { BpmMin: not null, BpmMax: not null };
        BpmMin = meta.BpmMin;
        BpmMax = meta.BpmMax;
        Scene = meta.Scene;
        SceneEgg = meta.SceneEgg;
        BackgroundVideoOpacity = meta.BackgroundVideoOpacity;
        SearchKeywordsText = meta.SearchKeywords is { } keywords ? string.Join(", ", keywords) : string.Empty;
        HideMode = meta.HideMode;
        HideRatingOverride = meta.HideRatingOverride;
        HideMessage = meta.HideMessage;

        foreach (var row in Maps)
        {
            row.PropertyChanged -= OnMapChanged;
        }

        Maps.Clear();
        foreach (var difficulty in ChartDifficultyExtensions.GetValues())
        {
            if (!meta.Maps.TryGetValue(ChartFiles.MapName(difficulty), out var map))
            {
                continue;
            }

            var row = new DifficultyEditViewModel(difficulty, map.Rating, string.Join(", ", map.Charters));
            row.PropertyChanged += OnMapChanged;
            Maps.Add(row);
        }

        HasHiddenDifficulty = meta.Maps.ContainsKey(ChartFiles.MapName(ChartDifficulty.Hidden));

        RebuildFilesDisplay();

        _loading = false;
        _dirty = false;

        WarnOnMissingFiles();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_filePath is null)
        {
            return;
        }

        var bytes = MessagePackSerialization.SerializeManifest(BuildManifest());
        if (await FileSystemService.TryWriteFileAtomicAsync(_filePath, bytes).ConfigureAwait(true))
        {
            _dirty = false;
            NotificationService.SuccessLight(Notification_Content_Epk_Save_Success, Name);
            CloseRequested?.Invoke();
        }
        else
        {
            NotificationService.ErrorLight(Notification_Content_Epk_Save_Failed, Name);
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        if (_dirty &&
            await MessageBoxService.WarningConfirmAsync(MessageBox_Content_Epk_Discard_Confirm).ConfigureAwait(true) is not MessageBoxResult.Yes)
        {
            return;
        }

        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void RefreshFiles()
    {
        if (_folder is null)
        {
            return;
        }

        var refreshed = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, size) in FileSystemService.GetFileSizes(_folder))
        {
            if (string.Equals(name, ChartFiles.ManifestFileName, StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var version = _files.TryGetValue(name, out var existing) ? existing.Version : 1;
            refreshed[name] = new ManifestFileEntry { Version = version, Size = size };
        }

        _files = refreshed;
        RebuildFilesDisplay();
        _dirty = true;
    }

    private bool CanSave =>
        !Name.IsNullOrWhiteSpace()
        && !Author.IsNullOrWhiteSpace()
        && (!IsBpmRange || (BpmMin is { } min && BpmMax is { } max && min <= max));

    private Manifest BuildManifest()
    {
        var original = _original!;
        var meta = original.Meta;

        var maps = new Dictionary<string, ManifestMap>(meta.Maps.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, map) in meta.Maps)
        {
            var edited = Maps.FirstOrDefault(row => string.Equals(ChartFiles.MapName(row.Difficulty), key, StringComparison.OrdinalIgnoreCase));
            maps[key] = new ManifestMap
            {
                Rating = edited?.Rating ?? map.Rating,
                Charters = edited is not null ? SplitList(edited.ChartersText) : map.Charters,
                PredictedRating = map.PredictedRating
            };
        }

        var editedMeta = new ManifestMeta
        {
            Name = Name,
            NameRomanized = NullIfBlank(NameRomanized),
            Author = Author,
            Description = NullIfBlank(Description),
            SafeForStreamer = SafeForStreamer,
            Bpm = Bpm ?? 0,
            BpmMin = IsBpmRange ? BpmMin : null,
            BpmMax = IsBpmRange ? BpmMax : null,
            Scene = Scene,
            SceneEgg = NullIfBlank(SceneEgg),
            BackgroundVideoOpacity = BackgroundVideoOpacity,
            SearchKeywords = SplitListOrNull(SearchKeywordsText),
            Maps = maps,
            HideMode = HasHiddenDifficulty ? NullIfBlank(HideMode) : meta.HideMode,
            HideRatingOverride = HasHiddenDifficulty ? NullIfBlank(HideRatingOverride) : meta.HideRatingOverride,
            HideMessage = HasHiddenDifficulty ? NullIfBlank(HideMessage) : meta.HideMessage,
            CoverDominantColor = meta.CoverDominantColor,
            Uploader = meta.Uploader,
            CreatedAt = meta.CreatedAt,
            UpdatedAt = meta.UpdatedAt
        };

        return new Manifest
        {
            Schema = Manifest.CurrentSchema,
            Cid = original.Cid,
            Meta = editedMeta,
            Files = new Dictionary<string, ManifestFileEntry>(_files, StringComparer.OrdinalIgnoreCase)
        };
    }

    private void RebuildFilesDisplay()
    {
        Files.Clear();
        foreach (var (name, entry) in _files.OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            Files.Add(new EpkFileEntry(name, entry.Size));
        }
    }

    private void WarnOnMissingFiles()
    {
        if (_folder is null)
        {
            return;
        }

        var present = FileSystemService.GetFileSizes(_folder);
        var missing = _files.Keys.Count(name => !present.ContainsKey(name));
        if (missing > 0)
        {
            NotificationService.WarningLight(Notification_Content_Epk_Open_FilesMissing, missing);
        }
    }

    private void OnMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_loading)
        {
            _dirty = true;
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!_loading)
        {
            _dirty = true;
        }
    }

    private static string? NullIfBlank(string? value) =>
        value.IsNullOrWhiteSpace() ? null : value.Trim();

    private static string[] SplitList(string? value) =>
        value.IsNullOrWhiteSpace() ? [] : [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    private static string[]? SplitListOrNull(string? value) =>
        value.IsNullOrWhiteSpace() ? null : [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    #region Injections

    public required IFileSystemService FileSystemService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
