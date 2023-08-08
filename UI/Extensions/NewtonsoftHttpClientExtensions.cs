using System.Net.Http;
using Newtonsoft.Json;

namespace MuseDashModToolsUI.Extensions;

public static class NewtonsoftHttpClientExtensions
{
    public async static Task<T> GetFromJsonAsync<T>(this HttpClient httpClient, string uri)
    {
        var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json)!;
    }
}