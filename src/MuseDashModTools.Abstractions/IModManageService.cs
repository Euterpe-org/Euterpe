using DynamicData;

namespace MuseDashModTools.Abstractions;

public interface IModManageService
{
    Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache);
}