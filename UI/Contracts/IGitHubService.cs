using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IGitHubService
{
    Task<List<Mod>> GetModsAsync();
    Task DownloadModAsync(string link, string path);
    Task DownloadMelonLoader(string path, IProgress<double> downloadProgress);
    Task CheckUpdates();
}