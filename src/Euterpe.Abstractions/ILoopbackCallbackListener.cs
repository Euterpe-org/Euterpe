using Euterpe.Contracts.Account;

namespace Euterpe.Abstractions;

public interface ILoopbackCallbackListener : IDisposable
{
    /// <summary>
    ///     The loopback port bound on 127.0.0.1, assigned by the OS.
    /// </summary>
    int Port { get; }

    /// <summary>
    ///     Wait for the single OAuth callback request and parse its query parameters.
    /// </summary>
    Task<LoopbackCallbackResult> WaitForCallbackAsync(CancellationToken cancellationToken = default);
}
