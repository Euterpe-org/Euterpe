using Mapster;

namespace MuseDashModToolsUI.Models.DTOs;

public sealed partial class ModDto : ObservableObject
{
    private readonly Mod _mod;
    [ObservableProperty] private bool _isDisabled;
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? DownloadLink { get; set; }
    public string? HomePage { get; set; }
    public string? ConfigFile { get; set; }
    public string[]? GameVersion { get; set; }
    public string? Description { get; set; }
    public List<string>? DependentMods { get; set; }
    public List<string>? DependentLibs { get; set; }
    public List<string>? IncompatibleMods { get; set; }
    public string? SHA256 { get; set; }
    public string? LocalVersion { get; set; }
    public UpdateState State { get; set; }
    public bool IsIncompatible { get; set; }
    public bool IsUpdatable { get; set; }
    public string? FileName { get; set; }
    public bool IsLocal { get; set; }
    public bool IsInstallable { get; set; }

    public bool IsTracked { get; set; }
    public bool IsShaMismatched { get; set; }
    public bool IsDuplicated { get; set; }
    public string? DuplicatedModNames { get; set; }

    public string? XamlDescription { get; set; }

    public bool IsValidConfigFile { get; set; }

    public bool IsValidHomePage { get; set; }

    public string? CompatibleGameVersion { get; set; }

    public bool HasDependency { get; set; }

    public string? DependencyNames { get; set; }

    public ModDto(Mod mod)
    {
        _mod = mod;
        _mod.Adapt(this);
    }

    public string FileNameExtended(bool reverse = false) => FileName + ((reverse ? !IsDisabled : IsDisabled) ? ".disabled" : string.Empty);

    [RelayCommand]
    private void Apply() => this.Adapt(_mod);

    [RelayCommand]
    private void Discard() => _mod.Adapt(this);
}