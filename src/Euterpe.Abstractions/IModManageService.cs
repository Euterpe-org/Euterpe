namespace Euterpe.Abstractions;

public interface IModManageService
{
    IObservable<IChangeSet<ModDto, string>> Connect();
    Task InitializeModsAsync();
    ModDto? FindModByName(string name);
    Task InstallModAsync(ModDto mod);
    Task UpdateModAsync(ModDto mod);
    Task ReinstallModAsync(ModDto mod);
    Task UninstallModAsync(ModDto mod);
    Task ToggleModAsync(ModDto mod);
    Task UpdateAllModsAsync();
}