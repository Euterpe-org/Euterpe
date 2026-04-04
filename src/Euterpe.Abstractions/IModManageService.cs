using DotNext.Threading;

namespace Euterpe.Abstractions;

public interface IModManageService
{
    AsyncManualResetEvent Ready { get; }
    Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache);
    ModDto? FindModByName(string name);
    Task InstallModAsync(ModDto mod);
    Task UpdateModAsync(ModDto mod);
    Task ReinstallModAsync(ModDto mod);
    Task UninstallModAsync(ModDto mod);
    Task ToggleModAsync(ModDto mod);
}