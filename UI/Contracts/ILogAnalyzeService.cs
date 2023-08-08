namespace MuseDashModToolsUI.Contracts;

public interface ILogAnalyzeService
{
    Task AnalyzeLog();
    Task<bool> CheckPirate();
    Task<bool> CheckMelonLoaderVersion();
    Task<string> LoadLog();
}