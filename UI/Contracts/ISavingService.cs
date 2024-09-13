namespace MuseDashModToolsUI.Contracts;

public interface ISavingService
{
    Task LoadSettingAsync();
    Task SaveSettingAsync();
}