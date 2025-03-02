using System.Globalization;

namespace MuseDashModTools.Models;

public sealed class Language(string cultureName)
{
    public string Name { get; } = cultureName;
    private string DisplayName { get; } = CultureInfo.GetCultureInfo(cultureName).DisplayName;
    public override string ToString() => $"{Name} - {DisplayName}";
}