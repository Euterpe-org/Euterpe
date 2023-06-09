using System.Threading.Tasks;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ISettingService
{
    public Setting Settings { get; set; }
    Task OnChoosePath();
}