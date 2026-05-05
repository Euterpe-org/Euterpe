namespace Euterpe.Abstractions;

public interface IGameModTemplateInstaller
{
    /// <summary>
    ///     Check the mod template is installed.
    /// </summary>
    Task<bool> CheckInstalledAsync();

    /// <summary>
    ///     Install the mod template.
    /// </summary>
    Task InstallAsync();

    /// <summary>
    ///     Uninstall the mod template.
    /// </summary>
    Task UninstallAsync();
}