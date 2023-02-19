using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _client;
    private readonly string _baseLink = "MDModsDev/ModLinks/dev/";
    
    public GitHubService(HttpClient client)
    {
        _client = client;
    }
    public async Task<List<WebModInfo>> GetModsAsync()
    {
        var result = Enumerable.Empty<WebModInfo>();
        try
        {
            result = await _client.GetFromJsonAsync<List<WebModInfo>>(
                "https://raw.githubusercontent.com/" + _baseLink +
                "ModLinks.json");
        }
        catch (Exception)
        {
            result = await _client.GetFromJsonAsync<List<WebModInfo>>("https://raw.fastgit.org/" + _baseLink +
                                                                      "ModLinks.json");
        }
        
        return result!.ToList();
    }
}