using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Models;

namespace UI.Contracts;

public interface IGitHubService
{
    Task<List<Mod>> GetModsAsync();
    Task DownloadModAsync(string link, string path);
    Task DownloadMelonLoader(string link, string path);
    void CheckUpdates();
}