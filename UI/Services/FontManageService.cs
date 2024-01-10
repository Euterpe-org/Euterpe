using System.ComponentModel;
using Avalonia.Media;
using SkiaSharp;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public sealed partial class FontManageService : IFontManageService, INotifyPropertyChanged
{
    private static readonly SKFontManager _skFontManager = SKFontManager.Default;
    private readonly ILogger _logger;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    public FontFamily this[string _] => new(SavingService.Settings.FontName!);
    public static string DefaultFont => SKTypeface.Default.FamilyName;

    public FontManageService(ILogger logger)
    {
        _logger = logger;
        GetAvailableFonts();
    }

    public List<string> AvailableFonts { get; private set; } = new();

    public void SetFont(string fontName)
    {
        if (SavingService.Settings.FontName == fontName)
        {
            return;
        }

        SavingService.Settings.FontName = fontName;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        _logger.Information("Font changed to {FontName}", fontName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}