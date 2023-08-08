namespace MuseDashModToolsUI.Contracts;

public interface IFontManageService
{
    List<string> AvailableFonts { get; }
    void SetFont(string fontName);
}