using System.Collections.Generic;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IMessageBoxService
{
    Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Success);
    Task<ButtonResult> CreateNoticeMessageBox(string content, Icon icon = Icon.Info);
    Task<ButtonResult> CreateSuccessMessageBox(string content, Icon icon = Icon.Success);
    Task<ButtonResult> CreateWarningMessageBox(string content, Icon icon = Icon.Warning);
    Task<ButtonResult> CreateErrorMessageBox(string title, string content);
    Task<ButtonResult> CreateErrorMessageBox(string content);
    Task<ButtonResult> CreateAnalyzeSuccessMessageBox(string content);
    Task<bool> CreateConfirmMessageBox(string title, string content);
    Task<bool> CreateConfirmMessageBox(string content);
    Task<string> CreateCustomMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);
    Task<string> CreateCustomConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info);
    Task<string> CreateCustomConfirmMessageBox(string content, int buttonCount);
    Task<string> CreateCustomMarkDownMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);
    Task<string> CreateCustomMarkDownConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info);
    Task<string> CreateCustomMarkDownConfirmMessageBox(string content, int buttonCount);
}