using System.Threading.Tasks;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ISavingService
{
    public Setting Settings { get; }
    Task InitializeSettings();
    Task SaveSettings();
    Task OnChoosePath();
}