using MuseDashModToolsUI.Localization.ModDescription;

namespace MuseDashModToolsUI.Utils;

public class ModDescriptionProvider
{
    public string? this[string resourceKey] => Resources_ModDescription.ResourceManager.GetString(resourceKey, Resources_ModDescription.Culture)?
        .Replace("\\n", "\n");

    public static ModDescriptionProvider Instance { get; } = new();
}