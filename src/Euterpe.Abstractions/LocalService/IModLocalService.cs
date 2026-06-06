namespace Euterpe.Abstractions;

public interface IModLocalService
{
    string[] GetModFilePaths();
    string[] GetLibFilePaths();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    Task<LibDto> LoadLibFromPathAsync(string filePath);
}