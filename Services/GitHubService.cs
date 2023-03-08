using System;
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
    private const string SecondaryLink = "https://raw.fastgit.org/";
    
    public GitHubService(HttpClient client)
    {
        _client = client;
    }
    public async Task<List<Mod>> GetModsAsync()
    {
        List<Mod> mods;
        try
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(
                PrimaryLink + BaseLink +
                "ModLinks.json"))!;
        }
        catch (Exception ex)
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(
                SecondaryLink + BaseLink +
                "ModLinks.json"))!;
        }

        return mods;

    }

    public async Task DownloadModAsync(string link, string path)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.GetAsync(PrimaryLink + BaseLink + link);
        }
        catch (Exception)
        {
            result = await _client.GetAsync(SecondaryLink + BaseLink + link);
        }
        await using var fs = new FileStream(path, FileMode.CreateNew);
        await result.Content.CopyToAsync(fs);
    }
    
}