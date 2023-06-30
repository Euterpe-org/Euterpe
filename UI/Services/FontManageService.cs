using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using MuseDashModToolsUI.Contracts;
using Serilog;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MuseDashModToolsUI.Services;

public class FontManageService : IFontManageService, INotifyPropertyChanged
{
    public ILogger? Logger { get; init; }
    public ISettingService? SettingService { get; init; }

    public FontFamily this[string _] => new(SettingService.Settings.FontName!);

    public List<string> AvailableFonts => GetAvailableFonts();

    public void SetFont(string fontName)
    {
        if (SettingService.Settings.FontName == fontName) return;
        SettingService.Settings.FontName = fontName;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        Logger.Information("Font changed to {FontName}", fontName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static List<string> GetAvailableFonts() => FontManager.Current.SystemFonts.Select(x => x.Name).Order().ToList();
}