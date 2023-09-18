namespace MuseDashModToolsUI.Contracts;

public interface IFontManageService
{
    List<string> AvailableFonts { get; }

    /// <summary>
    ///     Set Font
    /// </summary>
    /// <param name="fontName"></param>
    void SetFont(string fontName);
}