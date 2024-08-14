using MuseDashModToolsUI.Localization.ModDescription;

namespace MuseDashModToolsUI.Utils;

public static class ModDescriptionProvider
{
    public static string GetDescription(ModDto mod) => Resources_ModDescription.ResourceManager
        .GetString(mod.Name, Resources_ModDescription.Culture)?.Replace("\\n", "\n") ?? mod.Description;
}