namespace Euterpe.Models.Mods;

public sealed class LibDto
{
    public bool IsLocal { get; init; }

    #region Lib Properties

    public required string Name { get; init; }
    public required string FileName { get; init; }
    public required string SHA256 { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;

    #endregion Lib Properties
}
