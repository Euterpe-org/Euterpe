using MuseDashModTools.Localization;

namespace MuseDashModTools.Utils;

public static class ModDescriptionProvider
{
    public static string GetDescription(ModDto mod) => ModDescription.ResourceManager
        .GetString(mod.Name, ModDescription.Culture)?.Replace("\\n", "\n") ?? mod.Description;
}