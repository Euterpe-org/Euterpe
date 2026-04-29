namespace Euterpe.Abstractions;

public interface ISettingService
{
    void Load();
    Task LoadAsync();
    void Save();
    Task SaveAsync();
    Task ValidateAsync();
}