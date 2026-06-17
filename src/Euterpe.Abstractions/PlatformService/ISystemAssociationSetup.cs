namespace Euterpe.Abstractions;

public interface ISystemAssociationSetup
{
    /// <summary>
    ///     Fixed deep link scheme shared by all platforms.
    /// </summary>
    const string DeepLinkScheme = "euterpe";

    /// <summary>
    ///     Register the deep link scheme and file type associations for the current platform.
    /// </summary>
    /// <param name="processPath"></param>
    Task RegisterAsync(string processPath);
}
