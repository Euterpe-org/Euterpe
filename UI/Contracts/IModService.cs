using System.Collections.ObjectModel;
using DynamicData;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IModService
{
    /// <summary>
    ///     Compare pass in mod version with web mod
    /// </summary>
    /// <param name="modName"></param>
    /// <param name="modVersion"></param>
    /// <returns>OutDated</returns>
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