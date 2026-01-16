namespace Euterpe.Abstractions;

public interface ISettingService
{
    Task LoadAsync();
    Task SaveAsync();
    Task ValidateAsync();
}