using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IModService
{
    Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods);
    int CompareVersion(string modName, string modVersion);
    Task OnInstallMod(Mod item);
    Task OnReinstallMod(Mod item);
    Task OnToggleMod(Mod item);
    Task OnDeleteMod(Mod item);
}