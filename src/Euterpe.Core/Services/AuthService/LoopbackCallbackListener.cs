using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed class LoopbackCallbackListener : ILoopbackCallbackListener
{
    private const string DonePageUrl = "https://euterpe-org.com/auth/app/done";

    private readonly TcpListener _listener;

    public LoopbackCallbackListener()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
    }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public async Task<LoopbackCallbackResult> WaitForCallbackAsync(CancellationToken cancellationToken = default)
    {
        using var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

        var stream = client.GetStream();
        await using (stream.ConfigureAwait(false))
        {
            var requestLine = await ReadRequestLineAsync(stream, cancellationToken).ConfigureAwait(false);
            var result = ParseCallback(requestLine);
            await WriteResponseAsync(stream, result, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }

    private static async Task<string> ReadRequestLineAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        var text = Encoding.ASCII.GetString(buffer, 0, read);
        var lineEnd = text.IndexOf('\r');
        return lineEnd >= 0 ? text[..lineEnd] : text;
    }

    private static async Task WriteResponseAsync(NetworkStream stream, LoopbackCallbackResult result, CancellationToken cancellationToken)
    {
        // On success redirect the browser to the styled, localized landing page hosted by the web app.
        // Failures stay on the loopback origin: the app surfaces its own error UI, the browser just needs to close.
        var header = result.Error.IsNullOrEmpty() && !result.Code.IsNullOrEmpty()
            ? Encoding.ASCII.GetBytes(
                "HTTP/1.1 302 Found\r\n"
                + $"Location: {DonePageUrl}\r\n"
                + "Connection: close\r\n\r\n")
            : Encoding.ASCII.GetBytes(
                "HTTP/1.1 400 Bad Request\r\n"
                + "Connection: close\r\n\r\n");

        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
    }

    private static LoopbackCallbackResult ParseCallback(string requestLine)
    {
        // requestLine: "GET /callback?code=...&state=... HTTP/1.1"
        var parts = requestLine.Split(' ');
        if (parts.Length < 2)
        {
            return new LoopbackCallbackResult(null, null, "invalid_request");
        }

        var queryStart = parts[1].IndexOf('?');
        if (queryStart < 0)
        {
            return new LoopbackCallbackResult(null, null, null);
        }

        var query = HttpUtility.ParseQueryString(parts[1][(queryStart + 1)..]);
        return new LoopbackCallbackResult(query["code"], query["state"], query["error"]);
    }

    public void Dispose() => _listener.Dispose();
}
