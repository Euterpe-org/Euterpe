using System.Globalization;
using Euterpe.Localization;

namespace Euterpe.Tests;

public static class GlobalHooks
{
    [Before(TestSession)]
    public static void UseInvariantCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        LocalizationManager.Culture = CultureInfo.InvariantCulture;
    }
}
