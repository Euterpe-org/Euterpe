using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;

namespace MuseDashModToolsUI.Contracts
{
    public interface IDialogueService
    {
        Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button, Icon icon);
        Task<ButtonResult> CreateErrorMessageBox(string title, string content);
        Task<ButtonResult> CreateErrorMessageBox(string content);
        Task<bool> CreateConfirmMessageBox(string title, string content);
        Task<bool> CreateConfirmMessageBox(string content);
    }
}
