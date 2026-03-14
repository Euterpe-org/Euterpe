using Euterpe.Models.Dependencies;
using static Euterpe.Shared.DependencyConstants;

namespace Euterpe.Core;

internal sealed partial class DependencyAcquireService : IDependencyAcquireService
{
    private const int MaxRetries = 3;

    private DependencySpec[] MelonLoaderDependencies =>
    [
        new("MelonLoader", MelonLoader.Url, Config.MelonLoaderZipPath, MelonLoader.ZipHash),
        new("UnityDependency", UnityRuntime.Url, Config.UnityDependencyZipPath, UnityRuntime.ZipHash),
        new("Cpp2IL", Cpp2IL.ExecutableUrl, Config.Cpp2ILExecutablePath, Cpp2IL.ExecutableHash),
        new("Cpp2IL Plugin", Cpp2IL.PluginUrl, Config.Cpp2ILPluginPath, Cpp2IL.PluginHash)
    ];

    public async Task AcquireForMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var dependency in MelonLoaderDependencies)
        {
            await EnsureValidAsync(dependency, onDownloadStarted, progress, cancellationToken).ConfigureAwait(false);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<DependencyAcquireService> Logger { get; init; }

    #endregion Injections
}