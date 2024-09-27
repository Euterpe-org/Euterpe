using DynamicData;

namespace MuseDashModTools.Contracts;

public interface IModManageService
{
    Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache);
}