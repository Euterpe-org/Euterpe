namespace MuseDashModToolsUI.Services;

public partial class FontManageService
{
    private void GetAvailableFonts()
    {
        AvailableFonts = _skFontManager.GetFontFamilies().Order().ToList();
        _logger.Information("Available fonts loaded: {InstalledFonts}", string.Join(",", AvailableFonts));
    }
}