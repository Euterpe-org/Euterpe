using System.Net.Http.Json;
using static Euterpe.Core.JsonContexts.CamelCaseJsonContext;
using static Euterpe.Shared.EuterpeCdn;

namespace Euterpe.Core.Http.Clients;

public sealed class EuterpeCdnClient(HttpClient client)
{
    public async Task<Mod[]> FetchModListAsync(CancellationToken cancellationToken = default) =>
        await client.GetFromJsonAsync<Mod[]>(Assets.ModsJsonUrl, Default.ModArray, cancellationToken).ConfigureAwait(false) ?? [];

    public async Task<Lib[]> FetchLibListAsync(CancellationToken cancellationToken = default) =>
        await client.GetFromJsonAsync<Lib[]>(Assets.LibsJsonUrl, Default.LibArray, cancellationToken).ConfigureAwait(false) ?? [];
}