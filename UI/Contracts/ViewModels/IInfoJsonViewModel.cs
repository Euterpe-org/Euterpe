namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface IInfoJsonViewModel
{
    string[] HideBmsMode { get; set; }
    string[] HideBmsDifficulty { get; set; }
    void Initialize();
}