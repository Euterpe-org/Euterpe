using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using MuseDashModToolsUI.Contracts;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MuseDashModToolsUI.Services;

public class FontManageService : IFontManageService
{
    public ISettingService? SettingService { get; init; }
    public List<string> AvailableFonts => GetAvailableFonts();

    public void SetFont(string fontName)
    {
        SettingService.Settings.FontName = fontName;
    }

    private static List<string> GetAvailableFonts() => FontManager.Current.SystemFonts.Select(x => x.Name).Order().ToList();
}