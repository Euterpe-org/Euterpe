namespace Euterpe.Abstractions;

public interface IDeepLinkSetup
{
    /// <summary>
    ///     Fixed deep link scheme shared by all platforms.
    /// </summary>
    const string DeepLinkScheme = "euterpe";

    /// <summary>
    ///     Setup deep link handler for the current platform.
    /// </summary>
    /// <param name="processPath"></param>
    Task SetupDeepLinkAsync(string processPath);
}
