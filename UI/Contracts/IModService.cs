using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IModService
{
    bool CompareVersion(string modName, string modVersion);
    Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods);
    Task OnInstallMod(Mod item);
    Task OnReinstallMod(Mod item);
    Task OnToggleMod(Mod item);
    Task OnDeleteMod(Mod item);
}