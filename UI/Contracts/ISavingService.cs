namespace MuseDashModTools.Contracts;

public interface ISavingService
{
    Task LoadSettingAsync();
    Task SaveSettingAsync();
}