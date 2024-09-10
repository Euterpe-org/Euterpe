namespace MuseDashModToolsUI.Services.Downloads;

public sealed class CustomDownloadService : ICustomDownloadService
{
    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    public Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}