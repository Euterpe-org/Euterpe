namespace MuseDashModTools.Abstractions;

public interface ISavingService
{
    Task LoadSettingAsync();
    Task SaveSettingAsync();
}