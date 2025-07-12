using System.Globalization;

namespace MuseDashModTools.Models;

public sealed class Language
{
    public string Name { get; }
    private string DisplayName { get; }

    private Language(string cultureName)
    {
        Name = cultureName;
        DisplayName = CultureInfo.GetCultureInfo(cultureName).DisplayName;
    }

    public override string ToString() => $"{Name} - {DisplayName}";

    public static implicit operator Language(string cultureName) => new(cultureName);
}