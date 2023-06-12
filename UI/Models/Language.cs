using System.Globalization;

namespace MuseDashModToolsUI.Models;

public class Language
{
    public string? Name { get; set; }
    private string? DisplayName { get; }

    public string FullName => $"{Name} - {DisplayName}";

    public Language(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public Language(CultureInfo cultureInfo)
    {
        Name = cultureInfo.Name;
        DisplayName = cultureInfo.DisplayName;
    }
}