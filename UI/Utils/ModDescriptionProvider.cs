using MuseDashModToolsUI.Localization.ModDescription;

namespace MuseDashModToolsUI.Utils;

public static class ModDescriptionProvider
{
    public static string GetDescription(ModDto mod) => Resources.ResourceManager
        .GetString(mod.Name, Resources.Culture)?.Replace("\\n", "\n") ?? mod.Description;
}