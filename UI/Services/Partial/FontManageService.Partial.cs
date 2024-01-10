namespace MuseDashModToolsUI.Services;

public sealed partial class FontManageService
{
    /// <summary>
    ///     Get available fonts
    /// </summary>
    private void GetAvailableFonts()
    {
        AvailableFonts = _skFontManager.GetFontFamilies().Order().ToList();
        _logger.Information("Available fonts loaded: {InstalledFonts}", string.Join(",", AvailableFonts));
    }
}