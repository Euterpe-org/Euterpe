namespace Euterpe.Abstractions;

public interface IModManageService
{
    // Stream + lifecycle
    IObservable<IChangeSet<ModDto, string>> Connect();
    Task InitializeModsAsync();

    // Lookup
    ModDto? FindModByName(string name);

    // Single-mod operations
    Task InstallModAsync(ModDto mod);
    Task UpdateModAsync(ModDto mod);
    Task ReinstallModAsync(ModDto mod);
    Task UninstallModAsync(ModDto mod);
    Task ToggleModAsync(ModDto mod);

    // Deep-link entry points
    Task InstallModByNameAsync(string name);
    Task UpdateModByNameAsync(string name);
    Task UninstallModByNameAsync(string name);

    // Bulk operations
    Task<int> UpdateAllModsAsync();
    Task ImportModsAsync(IReadOnlyList<string> filePaths);

    // Disk reconciliation
    Task ReconcileModsAsync();
}
