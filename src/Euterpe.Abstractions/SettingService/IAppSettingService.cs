namespace Euterpe.Abstractions;

public interface IAppSettingService
{
    void Load();
    Task LoadAsync();
    void Save();
    Task SaveAsync();
    Task ValidateSteamAsync();
}
