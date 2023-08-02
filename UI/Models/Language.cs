using System.Globalization;

// ReSharper disable InconsistentNaming

namespace MuseDashModToolsUI.Models;

public class Language
{
    public string? Name { get; set; }
    public string? ThreeLetterISOLanguageName { get; set; }
    private string? DisplayName { get; }

    public string FullName => $"{Name} - {DisplayName}";

    public Language(CultureInfo cultureInfo)
    {
        Name = cultureInfo.Name;
        DisplayName = cultureInfo.DisplayName;
        ThreeLetterISOLanguageName = cultureInfo.ThreeLetterISOLanguageName;
    }
}