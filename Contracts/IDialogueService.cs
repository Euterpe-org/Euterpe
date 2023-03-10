using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;

namespace MuseDashModToolsUI.Contracts
{
    public interface IDialogueService
    {
        Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.None);
        Task<ButtonResult> CreateErrorMessageBox(string title, string content);
        Task<ButtonResult> CreateErrorMessageBox(string content);
    }
}
