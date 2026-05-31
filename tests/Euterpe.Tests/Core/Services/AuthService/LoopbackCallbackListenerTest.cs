using System.Net;
using System.Net.Sockets;
using System.Text;
using Euterpe.Contracts.Account;
using Euterpe.Core;

namespace Euterpe.Tests;

[Category("LoopbackCallbackListenerTests")]
[TestSubject(typeof(LoopbackCallbackListener))]
public sealed class LoopbackCallbackListenerTest
{
    private const string DonePageUrl = "https://euterpe-org.com/auth/app/done";

    private static async Task<(LoopbackCallbackResult Result, string Response)> RoundTripAsync(string requestLine, CancellationToken cancellationToken)
    {
        using var listener = new LoopbackCallbackListener();
        var callbackTask = listener.WaitForCallbackAsync(cancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.Port, cancellationToken);
        await using var stream = client.GetStream();

        await stream.WriteAsync(Encoding.ASCII.GetBytes($"{requestLine}\r\n\r\n"), cancellationToken);
        var result = await callbackTask;

        var buffer = new byte[1024];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        return (result, Encoding.ASCII.GetString(buffer, 0, read));
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithCodeAndState_ParsesResultAndRedirectsToDonePage(CancellationToken cancellationToken)
    {
        var (result, response) = await RoundTripAsync("GET /callback?code=abc&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(result.Code).IsEqualTo("abc");
        await Assert.That(result.State).IsEqualTo("xyz");
        await Assert.That(result.Error).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 302 Found");
        await Assert.That(response).Contains($"Location: {DonePageUrl}");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithError_ParsesErrorAndReturnsBadRequest(CancellationToken cancellationToken)
    {
        var (result, response) = await RoundTripAsync("GET /callback?error=access_denied&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(result.Error).IsEqualTo("access_denied");
        await Assert.That(result.Code).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 400 Bad Request");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithoutQuery_ReturnsEmptyResultAndBadRequest(CancellationToken cancellationToken)
    {
        var (result, response) = await RoundTripAsync("GET /callback HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(result.Code).IsNull();
        await Assert.That(result.State).IsNull();
        await Assert.That(result.Error).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 400 Bad Request");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithMalformedRequestLine_ReturnsInvalidRequest(CancellationToken cancellationToken)
    {
        var (result, response) = await RoundTripAsync("garbage", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(result.Error).IsEqualTo("invalid_request");
        await Assert.That(response).StartsWith("HTTP/1.1 400 Bad Request");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WhenCancelled_Throws(CancellationToken cancellationToken)
    {
        using var listener = new LoopbackCallbackListener();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await cts.CancelAsync();

        var act = async () => await listener.WaitForCallbackAsync(cts.Token);

        await Assert.That(act).Throws<OperationCanceledException>();
    }

    [Test]
    public async Task Port_IsAssignedByTheOperatingSystem()
    {
        using var listener = new LoopbackCallbackListener();

        await Assert.That(listener.Port).IsGreaterThan(0);
    }
}
