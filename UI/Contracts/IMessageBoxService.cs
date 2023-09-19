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
    ///     Create analyze success message box with formatted message
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<ButtonResult> FormatAnalyzeSuccessMessageBox(string content, params object[] args);

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
    ///     Create message box with Error title and formatted message
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<ButtonResult> FormatErrorMessageBox(string content, params object[] args);

    /// <summary>
    ///     Create message box with Notice title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Info</param>
    /// <returns></returns>
    Task<ButtonResult> NoticeMessageBox(string content, Icon icon = Icon.Info);

    /// <summary>
    ///     Create message box with Notice title and formatted message
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<ButtonResult> FormatNoticeMessageBox(string content, params object[] args);

    /// <summary>
    ///     Create message box with Success title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Success</param>
    /// <returns></returns>
    Task<ButtonResult> SuccessMessageBox(string content, Icon icon = Icon.Success);

    /// <summary>
    ///     Create message box with Success title and formatted message
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<ButtonResult> FormatSuccessMessageBox(string content, params object[] args);

    /// <summary>
    ///     Create message box with Warning title
    /// </summary>
    /// <param name="content"></param>
    /// <param name="icon">Default icon Warning</param>
    /// <returns></returns>
    Task<ButtonResult> WarningMessageBox(string content, Icon icon = Icon.Warning);

    /// <summary>
    ///     Create message box with Warning title and formatted message
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<ButtonResult> FormatWarningMessageBox(string content, params object[] args);

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
    ///     Create confirm message box with Notice title, formatted content,Yes No button and Info icon
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<bool> FormatNoticeConfirmMessageBox(string content, params object[] args);

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

    /// <summary>
    ///     Create confirm message box with Warning title, formatted content,Yes No button and Warning icon
    /// </summary>
    /// <param name="content"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<bool> FormatWarningConfirmMessageBox(string content, params object[] args);

    /// <summary>
    ///     Create custom message box with title, content, button definitions and icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="buttonDefinitions"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    Task<string> CustomMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);

    /// <summary>
    ///     Create custom confirm message box with title, content, button and icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="buttonCount"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    Task<string> CustomConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info);

    /// <summary>
    ///     Create custom confirm message box with Notice title, content, button and Info icon
    /// </summary>
    /// <param name="content"></param>
    /// <param name="buttonCount"></param>
    /// <returns></returns>
    Task<string> CustomConfirmMessageBox(string content, int buttonCount);

    /// <summary>
    ///     Create custom markdown message box with title, content, button definitions and icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="buttonDefinitions"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    Task<string> CustomMarkDownMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon);

    /// <summary>
    ///     Create custom markdown confirm message box with title, content, button and Info icon
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="buttonCount"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    Task<string> CustomMarkDownConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info);

    /// <summary>
    ///     Create custom markdown confirm message box with Notice title, content, button and Info icon
    /// </summary>
    /// <param name="content"></param>
    /// <param name="buttonCount"></param>
    /// <returns></returns>
    Task<string> CustomMarkDownConfirmMessageBox(string content, int buttonCount);
}