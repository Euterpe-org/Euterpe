using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;

namespace UI.Contracts;

public interface IDialogueService
{
    Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Success);
    Task<ButtonResult> CreateErrorMessageBox(string title, string content);
    Task<ButtonResult> CreateErrorMessageBox(string content);
    Task<bool> CreateConfirmMessageBox(string title, string content);
    Task<bool> CreateConfirmMessageBox(string content);
}