using System.Collections.ObjectModel;
using DynamicData;

namespace MuseDashModToolsUI.Contracts;

public interface IModService
{
    /// <summary>
    ///     Compare the pass in mod version with web mod version
    /// </summary>
    /// <param name="modName"></param>
    /// <param name="modVersion"></param>
    /// <returns>Is outdated</returns>
    bool CompareVersion(string modName, string modVersion);

    /// <summary>
    ///     Initialize Mod list
    /// </summary>
    /// <param name="sourceCache"></param>
    /// <param name="mods"></param>
    /// <returns></returns>
    Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods);

    /// <summary>
    ///     Install Mod
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task OnInstallMod(Mod item);

    /// <summary>
    ///     Reinstall Mod
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task OnReinstallMod(Mod item);

    /// <summary>
    ///     Toggle Mod
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task OnToggleMod(Mod item);

    /// <summary>
    ///     Delete Mod
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task OnDeleteMod(Mod item);
}