using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _client;
    
    private const string BaseLink = "MDModsDev/ModLinks/dev/";
    private const string PrimaryLink = "https://raw.githubusercontent.com/";
    
    public GitHubService(HttpClient client)
    {
        _client = client;
    }
    public async Task<List<Mod>> GetModsAsync()
    {
        return (await _client.GetFromJsonAsync<List<Mod>>(
            PrimaryLink + BaseLink +
            "ModLinks.json"))!;
        
    }

    public async Task DownloadModAsync(string link, string path)
    {
        var l = PrimaryLink + BaseLink + link;
        var result = await _client.GetAsync(PrimaryLink + BaseLink + link);
        await using var fs = new FileStream(path, FileMode.CreateNew);
        await result.Content.CopyToAsync(fs);
    }
    
}