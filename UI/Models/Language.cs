using System.Globalization;

namespace MuseDashModToolsUI.Models;

public sealed class Language
{
    public string? Name { get; set; }
    private string? DisplayName { get; }
    public string FullName => $"{Name} - {DisplayName}";

    public Language(CultureInfo cultureInfo)
    {
        Name = cultureInfo.Name;
        DisplayName = cultureInfo.DisplayName;
    }
}