using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Euterpe.Contracts.Account;

namespace Euterpe.Core.Http.Listeners;

internal sealed class LoopbackCallbackListener : ILoopbackCallbackListener
{
    private const string DonePageUrl = "https://euterpe-org.com/auth/app/done";
    private const string NotFoundResponse = "HTTP/1.1 404 Not Found\r\nConnection: close\r\n\r\n";

    private readonly TcpListener _listener;

    public LoopbackCallbackListener()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();

        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    }

    public int Port { get; }

    public async Task<LoopbackCallbackResult> WaitForCallbackAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            using var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

            var stream = client.GetStream();
            await using (stream.ConfigureAwait(false))
            {
                var requestLine = await ReadRequestLineAsync(stream, cancellationToken).ConfigureAwait(false);
                var result = ParseCallback(requestLine);

                if (result.Code.IsNullOrEmpty() && result.Error.IsNullOrEmpty())
                {
                    await WriteResponseAsync(stream, NotFoundResponse, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                await WriteResponseAsync(stream, BuildResponse(result), cancellationToken).ConfigureAwait(false);
                return result;
            }
        }
    }

    public void Dispose() => _listener.Dispose();

    private static string BuildResponse(LoopbackCallbackResult result) =>
        !result.Code.IsNullOrEmpty() && result.Error.IsNullOrEmpty()
            ? $"HTTP/1.1 302 Found\r\nLocation: {DonePageUrl}\r\nConnection: close\r\n\r\n"
            : "HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n";

    private static async Task WriteResponseAsync(NetworkStream stream, string response, CancellationToken cancellationToken) =>
        await stream.WriteAsync(Encoding.ASCII.GetBytes(response), cancellationToken).ConfigureAwait(false);

    private static async Task<string> ReadRequestLineAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken).ConfigureAwait(false);
            if (read is 0)
            {
                break;
            }

            total += read;
            var text = Encoding.ASCII.GetString(buffer, 0, total);
            var lineEnd = text.IndexOf("\r\n", StringComparison.Ordinal);
            if (lineEnd >= 0)
            {
                return text[..lineEnd];
            }
        }

        return Encoding.ASCII.GetString(buffer, 0, total);
    }

    private static LoopbackCallbackResult ParseCallback(string requestLine)
    {
        // requestLine: "GET /callback?code=...&state=... HTTP/1.1"
        var parts = requestLine.Split(' ');
        if (parts.Length < 2)
        {
            return new LoopbackCallbackResult(null, null, null);
        }

        var queryStart = parts[1].IndexOf('?');
        if (queryStart < 0)
        {
            return new LoopbackCallbackResult(null, null, null);
        }

        var query = HttpUtility.ParseQueryString(parts[1][(queryStart + 1)..]);
        return new LoopbackCallbackResult(query["code"], query["state"], query["error"]);
    }
}