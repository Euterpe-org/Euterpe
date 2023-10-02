using System.Globalization;
using System.Resources;
using MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public partial class LocalizationService
{
    /// <summary>
    ///     Get all available cultures from the resources
    /// </summary>
    private void GetAvailableCultures()
    {
        var rm = new ResourceManager(typeof(Resources));
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        var defaultCulture = CultureInfo.GetCultureInfo("en");

        foreach (var culture in cultures)
        {
            if (culture.Equals(CultureInfo.InvariantCulture))
            {
                AvailableLanguages.Add(new Language(defaultCulture));
                continue;
            }

            var rs = rm.GetResourceSet(culture, true, false);
            if (rs != null)
                AvailableLanguages.Add(new Language(culture));
        }

        _logger.Information("Available languages loaded: {AvailableLanguages}",
            string.Join(", ", AvailableLanguages.Select(x => x.Name)));
    }
}