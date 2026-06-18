using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Euterpe.Features.Charting;

[Register]
[AppSingleton]
public sealed partial class EpkEditorPanelViewModel : ViewModelBase
{
    private bool _loading;
    private string? _filePath;
    private string? _folder;
    private Manifest? _original;
    private Dictionary<string, ManifestFileEntry> _files = new(StringComparer.OrdinalIgnoreCase);

    public EpkEditorPanelViewModel() => SearchKeywords.CollectionChanged += OnEditCollectionChanged;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? NameRomanized { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial string Author { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial bool SafeForStreamer { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial int? Bpm { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(BpmRangeInvalid))]
    public partial bool IsBpmRange { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(BpmRangeInvalid))]
    public partial int? BpmMin { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(BpmRangeInvalid))]
    public partial int? BpmMax { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial string Scene { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SceneEgg { get; set; }

    [ObservableProperty]
    public partial float? BackgroundVideoOpacity { get; set; }

    [ObservableProperty]
    public partial bool HasHiddenDifficulty { get; set; }

    [ObservableProperty]
    public partial string? HideMode { get; set; }

    [ObservableProperty]
    public partial string? HideRatingOverride { get; set; }

    [ObservableProperty]
    public partial string? HideMessage { get; set; }

    [ObservableProperty]
    public partial string? FilePath { get; set; }

    [ObservableProperty]
    public partial string? FolderPath { get; set; }

    [ObservableProperty]
    public partial string? CoverPath { get; set; }

    [ObservableProperty]
    public partial string? CoverDominantColor { get; set; }

    [ObservableProperty]
    public partial bool HasVideo { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMissingFiles))]
    [NotifyPropertyChangedFor(nameof(MissingFilesMessage))]
    public partial int MissingFileCount { get; set; }

    [ObservableProperty]
    public partial string FilesSummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDirty { get; set; }

    public ObservableCollection<string> SearchKeywords { get; } = [];

    public ObservableCollection<DifficultyEditViewModel> Maps { get; } = [];

    public ObservableCollection<EpkFileEntry> Files { get; } = [];

    public bool CanSave =>
        !Name.IsNullOrWhiteSpace()
        && !Author.IsNullOrWhiteSpace()
        && !Scene.IsNullOrWhiteSpace()
        && Bpm is not null
        && !BpmRangeInvalid
        && Maps.Any(static map => map.Difficulty == ChartDifficulty.Hard)
        && Maps.All(static map => !map.Rating.IsNullOrWhiteSpace() && map.Charters.Any(static charter => !charter.IsNullOrWhiteSpace()));

    public bool BpmRangeInvalid =>
        IsBpmRange && (BpmMin is not { } min || BpmMax is not { } max || min > max);

    public bool HasMissingFiles => MissingFileCount > 0;

    public string MissingFilesMessage => string.Format(XAML.EpkEditor_MissingFiles, MissingFileCount);

    public event Action? CloseRequested;
    public event Action<string>? Saved;

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
        HideMode = meta.HideMode;
        HideRatingOverride = meta.HideRatingOverride;
        HideMessage = meta.HideMessage;

        FilePath = filePath;
        FolderPath = _folder;
        CoverDominantColor = meta.CoverDominantColor;
        CoverPath = ResolveCoverPath();

        LoadKeywords(meta.SearchKeywords);
        ReconcileMaps(preserveEdits: false);

        RebuildFilesDisplay();
        RecomputeMissingFiles();

        _loading = false;
        IsDirty = false;
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
            IsDirty = false;
            NotificationService.SuccessLight(Notification_Content_Epk_Save_Success, Name);
            if (_folder is { } folder)
            {
                Saved?.Invoke(folder);
            }
        }
        else
        {
            NotificationService.ErrorLight(Notification_Content_Epk_Save_Failed, Name);
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        if (IsDirty &&
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
        ReconcileMaps(preserveEdits: true);
        RecomputeMissingFiles();
        IsDirty = true;
    }

    private Manifest BuildManifest()
    {
        var original = _original!;
        var meta = original.Meta;

        var maps = new Dictionary<string, ManifestMap>(Maps.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var row in Maps)
        {
            var key = ChartFiles.MapName(row.Difficulty);
            maps[key] = new ManifestMap
            {
                Rating = row.Rating,
                Charters = CleanTags(row.Charters),
                PredictedRating = meta.Maps.TryGetValue(key, out var originalMap) ? originalMap.PredictedRating : null
            };
        }

        var keywords = CleanTags(SearchKeywords);

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
            SearchKeywords = keywords.Length > 0 ? keywords : null,
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

    private void LoadKeywords(string[]? keywords)
    {
        SearchKeywords.Clear();
        if (keywords is null)
        {
            return;
        }

        foreach (var keyword in keywords)
        {
            SearchKeywords.Add(keyword);
        }
    }

    // Rows track the .bms files on disk; the manifest only prefills each row's rating and charters, so a
    // difficulty dropped into or removed from the folder appears or drops out on refresh. Edits are kept.
    private void ReconcileMaps(bool preserveEdits)
    {
        var kept = preserveEdits
            ? Maps.ToDictionary(static row => row.Difficulty)
            : new Dictionary<ChartDifficulty, DifficultyEditViewModel>();

        foreach (var row in Maps)
        {
            row.PropertyChanged -= OnMapChanged;
            row.Charters.CollectionChanged -= OnEditCollectionChanged;
        }

        Maps.Clear();
        var manifestMaps = _original?.Meta.Maps;
        foreach (var difficulty in _files.ExistingDifficulties())
        {
            var manifestMap = manifestMaps?.GetValueOrDefault(ChartFiles.MapName(difficulty));
            var row = kept.GetValueOrDefault(difficulty)
                ?? new DifficultyEditViewModel(difficulty, manifestMap?.Rating ?? string.Empty, manifestMap?.Charters ?? []);
            row.PropertyChanged += OnMapChanged;
            row.Charters.CollectionChanged += OnEditCollectionChanged;
            Maps.Add(row);
        }

        HasHiddenDifficulty = Maps.Any(static row => row.Difficulty == ChartDifficulty.Hidden);
        NotifySaveCanExecuteChanged();
    }

    private string? ResolveCoverPath()
    {
        if (_folder is not { } folder)
        {
            return null;
        }

        return ChartFiles.CoverExtensions
            .Select(extension => ChartFiles.CoverName + extension)
            .Where(_files.ContainsKey)
            .Select(name => Path.Combine(folder, name))
            .FirstOrDefault();
    }

    private void RebuildFilesDisplay()
    {
        Files.Clear();
        foreach (var (name, entry) in _files.OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            Files.Add(new EpkFileEntry(name, entry.Size));
        }

        HasVideo = _files.ContainsKey(ChartFiles.VideoFileName);
        var totalSize = _files.Values.Sum(static entry => entry.Size);
        FilesSummary = string.Format(XAML.EpkEditor_Files_Summary, Files.Count, totalSize.ToReadableSize());
    }

    private void RecomputeMissingFiles()
    {
        if (_folder is null)
        {
            MissingFileCount = 0;
            return;
        }

        var present = FileSystemService.GetFileSizes(_folder);
        MissingFileCount = _files.Keys.Count(name => !present.ContainsKey(name));
    }

    private void OnMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_loading)
        {
            IsDirty = true;
        }

        NotifySaveCanExecuteChanged();
    }

    private void OnEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_loading)
        {
            IsDirty = true;
        }

        NotifySaveCanExecuteChanged();
    }

    private void NotifySaveCanExecuteChanged()
    {
        OnPropertyChanged(nameof(CanSave));
        SaveCommand.NotifyCanExecuteChanged();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!_loading && e.PropertyName != nameof(IsDirty))
        {
            IsDirty = true;
        }
    }

    private static string? NullIfBlank(string? value) =>
        value.IsNullOrWhiteSpace() ? null : value.Trim();

    private static string[] CleanTags(IEnumerable<string> tags) =>
        [.. tags.Select(static tag => tag.Trim()).Where(static tag => tag.Length > 0)];

    #region Injections

    public required IFileSystemService FileSystemService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
