using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using MuseDashModToolsUI.Contracts;
using Serilog;
using SkiaSharp;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MuseDashModToolsUI.Services;

public class FontManageService : IFontManageService, INotifyPropertyChanged
{
    private static readonly SKFontManager _skFontManager = SKFontManager.Default;
    private readonly ILogger _logger;
    public ISavingService? SavingService { get; init; }

    public FontFamily this[string _] => new(SavingService.Settings.FontName!);
    public static string DefaultFont => _skFontManager.GetFontFamilies()[0];

    public FontManageService(ILogger logger)
    {
        _logger = logger;
        GetAvailableFonts();
    }

    public List<string> AvailableFonts { get; private set; } = new();

    public void SetFont(string fontName)
    {
        if (SavingService.Settings.FontName == fontName) return;
        SavingService.Settings.FontName = fontName;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        _logger.Information("Font changed to {FontName}", fontName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void GetAvailableFonts()
    {
        AvailableFonts = _skFontManager.GetFontFamilies().Order().ToList();
        _logger.Information("Available fonts loaded: {InstalledFonts}", string.Join(",", AvailableFonts));
    }
}