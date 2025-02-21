namespace MuseDashModTools.Models;

public sealed class LibDto
{
    public bool IsLocal { get; set; }

    #region Lib Properties

    public string FileName { get; set; } = string.Empty;
    public string SHA256 { get; set; } = string.Empty;

    #endregion Lib Properties
}