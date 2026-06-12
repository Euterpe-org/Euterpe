namespace Euterpe.Core.Http.Clients;

internal sealed class EuterpeDownloadClient(HttpClient httpClient)
{
    public Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken = default) => httpClient.GetStreamAsync(url, cancellationToken);
}
