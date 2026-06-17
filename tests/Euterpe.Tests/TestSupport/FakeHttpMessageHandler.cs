using System.Net;

namespace Euterpe.Tests.TestSupport;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, int, HttpResponseMessage> _responder;
    public List<HttpRequestMessage> Requests { get; } = new();
    public List<Uri?> RequestUris { get; } = new();
    public List<string?> AuthorizationParameters { get; } = new();
    public int CallCount { get; private set; }

    public FakeHttpMessageHandler(HttpStatusCode status = HttpStatusCode.OK, string content = "")
        : this((_, _) => new HttpResponseMessage(status) { Content = new StringContent(content) })
    {
    }

    public FakeHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> responder) => _responder = responder;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestUris.Add(request.RequestUri);
        AuthorizationParameters.Add(request.Headers.Authorization?.Parameter);
        CallCount++;
        var response = _responder(request, CallCount);
        return Task.FromResult(response);
    }
}
