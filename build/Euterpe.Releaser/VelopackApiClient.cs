namespace Euterpe.Releaser;

internal sealed class VelopackApiClient : IDisposable
{
    private static readonly Uri ApiBaseAddress = new("https://euterpe-org.com/");

    private readonly HttpClient _httpClient;

    public VelopackApiClient()
    {
        _httpClient = new HttpClient(new XRequestIdHandler())
        {
            BaseAddress = ApiBaseAddress,
            Timeout = TimeSpan.FromMinutes(30)
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("ApiKey", Environment.GetEnvironmentVariable("EUTERPE_API_KEY"));
    }

    public void Dispose() => _httpClient.Dispose();

    public async Task<VelopackReleaseBase?> GetReleaseBaseAsync(string channel, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"api/workspace/velopack/{channel}/base",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync(
            content,
            ReleaserJsonContext.Default.VelopackReleaseBase,
            cancellationToken);
    }

    public async Task PublishAsync(SemVersion version, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/workspace/velopack/publish");

        request.Content = JsonContent.Create(
            new VelopackPublishRequest(version.ToString()),
            ReleaserJsonContext.Default.VelopackPublishRequest);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DownloadReleaseBaseAsync(
        string downloadPath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(downloadPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var destination = new FileStream(
            destinationPath,
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                Share = FileShare.None
            });

        await source.CopyToAsync(destination, cancellationToken);
    }

    public async Task UploadAssetAsync(
        string channel,
        SemVersion version,
        string assetType,
        string assetPath,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(assetPath);
        using var stream = new FileStream(
            assetPath,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                Share = FileShare.Read
            });

        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/workspace/velopack/{channel}/{version}/asset/{fileName}");

        request.Headers.Add("X-Asset-Type", assetType);
        request.Content = new StreamContent(stream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.ContentLength = stream.Length;

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Velopack API returned {(int)response.StatusCode} ({response.ReasonPhrase}): {responseBody}", null, response.StatusCode);
    }

    private sealed class XRequestIdHandler() : DelegatingHandler(new HttpClientHandler())
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Add("X-Request-Id", Guid.CreateVersion7().ToString());
            return base.SendAsync(request, cancellationToken);
        }
    }
}
