using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using MsBox.Avalonia.Enums;

namespace MuseDashModToolsUI.Contracts;

public interface IMessageBoxService
{
    Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Success);
    Task<ButtonResult> CreateSuccessMessageBox(string content);
    Task<ButtonResult> CreateErrorMessageBox(string title, string content);
    Task<ButtonResult> CreateErrorMessageBox(string content);
    Task<bool> CreateConfirmMessageBox(string title, string content);
    Task<bool> CreateConfirmMessageBox(string content);
    Task<string> CreateCustomMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);
    Task<string> CreateCustomConfirmMessageBox(string title, string content, int buttonCount, Icon icon);
    Task<string> CreateCustomConfirmMessageBox(string content, int buttonCount);
}