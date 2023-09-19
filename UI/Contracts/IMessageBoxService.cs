using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IMessageBoxService
{
    /// <summary>
    ///     Create message box with title, content, button and icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="icon"></param>
    /// <param name="button">Default button OK</param>
    /// <returns></returns>
    Task<ButtonResult> MessageBox(string title, string content, Icon icon, ButtonEnum button = ButtonEnum.Ok);

    /// <summary>
    ///     Create analyze success message box
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<ButtonResult> AnalyzeSuccessMessageBox(string content);

    /// <summary>
    ///     Create message box with Error title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Error</param>
    /// <returns></returns>
    Task<ButtonResult> ErrorMessageBox(string content, Icon icon = Icon.Error);

    /// <summary>
    ///     Create message box with Error title and formatted exception message
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    Task<ButtonResult> ErrorMessageBox(string content, Exception ex);

    /// <summary>
    ///     Create message box with Notice title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Info</param>
    /// <returns></returns>
    Task<ButtonResult> NoticeMessageBox(string content, Icon icon = Icon.Info);

    /// <summary>
    ///     Create message box with Success title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Success</param>
    /// <returns></returns>
    Task<ButtonResult> SuccessMessageBox(string content, Icon icon = Icon.Success);

    /// <summary>
    ///     Create message box with Warning title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Warning</param>
    /// <returns></returns>
    Task<ButtonResult> WarningMessageBox(string content, Icon icon = Icon.Warning);

    /// <summary>
    ///     Create confirm message box with title, content,Yes No button and icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    Task<bool> ConfirmMessageBox(string title, string content, Icon icon);

    /// <summary>
    ///     Create confirm message box with title, content,Yes No button and Info icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<bool> NoticeConfirmMessageBox(string title, string content);

    /// <summary>
    ///     Create confirm message box with Notice title, content,Yes No button and Info icon
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<bool> NoticeConfirmMessageBox(string content);

    /// <summary>
    ///     Create confirm message box with title, content,Yes No button and Warning icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<bool> WarningConfirmMessageBox(string title, string content);

    /// <summary>
    ///     Create confirm message box with Warning title, content,Yes No button and Warning icon
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<bool> WarningConfirmMessageBox(string content);

    Task<string> CustomMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);
    Task<string> CustomConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info);
    Task<string> CustomConfirmMessageBox(string content, int buttonCount);
    Task<string> CustomMarkDownMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);
    Task<string> CustomMarkDownConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info);
    Task<string> CustomMarkDownConfirmMessageBox(string content, int buttonCount);
}