namespace Euterpe.Abstractions;

public interface IModLocalService
{
    IEnumerable<string> GetModFilePaths();
    IEnumerable<string> GetLibFilePaths();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    Task<LibDto> LoadLibFromPathAsync(string filePath);
}