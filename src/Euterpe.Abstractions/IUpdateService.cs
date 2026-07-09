namespace Euterpe.Abstractions;

public interface IUpdateService
{
    Task<string?> CheckForUpdatesAsync();
    Task UpdateAsync(IProgress<int> progress);
}
