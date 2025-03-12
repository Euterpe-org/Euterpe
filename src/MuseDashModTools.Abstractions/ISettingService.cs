namespace MuseDashModTools.Abstractions;

public interface ISettingService
{
    Task LoadAsync();
    Task SaveAsync();
    Task ValidateAsync();
}