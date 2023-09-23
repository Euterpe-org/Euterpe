using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MuseDashModToolsUI.Services;

public partial class MessageBoxService : IMessageBoxService
{
    [UsedImplicitly]
    public Lazy<ISavingService>? SavingService { get; init; }

    #region Custom Message Box

    public async Task<string> CustomMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions, Icon icon)
    {
        var desktop = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var isMainWindow = desktop?.MainWindow is not null;
        var messageBox = MessageBoxManager
            .GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = title,
                ContentMessage = content.NormalizeNewline(),
                ButtonDefinitions = buttonDefinitions,
                Icon = icon,
                CanResize = true,
                Topmost = true,
                FontFamily = SavingService.Value.Settings.FontName,
                WindowStartupLocation = isMainWindow ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            });
        return isMainWindow ? await messageBox.ShowWindowDialogAsync(desktop!.MainWindow) : await messageBox.ShowAsync();
    }

    public async Task<string> CustomConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info)
    {
        var buttonDefinitions = GetButtonDefinition(buttonCount);
        return await CustomMessageBox(title, content, buttonDefinitions, icon);
    }

    public async Task<string> CustomConfirmMessageBox(string content, int buttonCount) =>
        await CustomConfirmMessageBox(MsgBox_Title_Notice, content, buttonCount);

    public async Task<string> CustomMarkDownMessageBox(string title, string content, IEnumerable<ButtonDefinition> buttonDefinitions,
        Icon icon)
    {
        var desktop = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var isMainWindow = desktop?.MainWindow is not null;
        var messageBox = MessageBoxManager
            .GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = title,
                ContentMessage = content,
                ButtonDefinitions = buttonDefinitions,
                Icon = icon,
                CanResize = true,
                Topmost = true,
                FontFamily = SavingService.Value.Settings.FontName,
                Markdown = true,
                WindowStartupLocation = isMainWindow ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            });
        return isMainWindow ? await messageBox.ShowWindowDialogAsync(desktop!.MainWindow) : await messageBox.ShowAsync();
    }

    public async Task<string> CustomMarkDownConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info)
    {
        var buttonDefinitions = GetButtonDefinition(buttonCount);
        return await CustomMarkDownMessageBox(title, content, buttonDefinitions, icon);
    }

    public async Task<string> CustomMarkDownConfirmMessageBox(string content, int buttonCount) =>
        await CustomMarkDownConfirmMessageBox(MsgBox_Title_Notice, content, buttonCount);

    #endregion

    #region Normal Message Box

    public async Task<ButtonResult> MessageBox(string title, string content, Icon icon, ButtonEnum button = ButtonEnum.Ok)
    {
        var desktop = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var isMainWindow = desktop?.MainWindow is not null;
        var messageBox = MessageBoxManager
            .GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = title,
                ContentMessage = content.NormalizeNewline(),
                ButtonDefinitions = button,
                Icon = icon,
                Topmost = true,
                FontFamily = SavingService.Value.Settings.FontName,
                WindowStartupLocation = isMainWindow ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            });
        return isMainWindow ? await messageBox.ShowWindowDialogAsync(desktop!.MainWindow) : await messageBox.ShowAsync();
    }

    public async Task<ButtonResult> AnalyzeSuccessMessageBox(string content) =>
        await MessageBox(MsgBox_Title_AnalyzeSuccess, content, Icon.Error);

    public async Task<ButtonResult> FormatAnalyzeSuccessMessageBox(string content, params object[] args) =>
        await AnalyzeSuccessMessageBox(string.Format(content, args));

    public async Task<ButtonResult> ErrorMessageBox(string content, Icon icon = Icon.Error) =>
        await MessageBox(MsgBox_Title_Failure, content, icon);

    public async Task<ButtonResult> ErrorMessageBox(string content, Exception ex) =>
        await ErrorMessageBox(string.Format(content, ex));

    public async Task<ButtonResult> ErrorMessageBox(Exception ex) => await ErrorMessageBox(ex.ToString());

    public async Task<ButtonResult> FormatErrorMessageBox(string content, params object[] args) =>
        await ErrorMessageBox(string.Format(content, args));

    public async Task<ButtonResult> NoticeMessageBox(string content, Icon icon = Icon.Info) =>
        await MessageBox(MsgBox_Title_Notice, content, icon);

    public async Task<ButtonResult> FormatNoticeMessageBox(string content, params object[] args) =>
        await NoticeMessageBox(string.Format(content, args));

    public async Task<ButtonResult> SuccessMessageBox(string content, Icon icon = Icon.Success) =>
        await MessageBox(MsgBox_Title_Success, content, icon);

    public async Task<ButtonResult> FormatSuccessMessageBox(string content, params object[] args) =>
        await SuccessMessageBox(string.Format(content, args));

    public async Task<ButtonResult> WarningMessageBox(string content, Icon icon = Icon.Warning) =>
        await MessageBox(MsgBox_Title_Warning, content, icon);

    public async Task<ButtonResult> FormatWarningMessageBox(string content, params object[] args) =>
        await WarningMessageBox(string.Format(content, args));

    public async Task<bool> ConfirmMessageBox(string title, string content, Icon icon)
    {
        var result = await CustomConfirmMessageBox(title, content, 2, icon);
        return result == MsgBox_Button_Yes;
    }

    public async Task<bool> NoticeConfirmMessageBox(string title, string content) => await ConfirmMessageBox(title, content, Icon.Info);
    public async Task<bool> NoticeConfirmMessageBox(string content) => await ConfirmMessageBox(MsgBox_Title_Notice, content, Icon.Info);

    public async Task<bool> FormatNoticeConfirmMessageBox(string content, params object[] args) =>
        await NoticeConfirmMessageBox(string.Format(content, args));

    public async Task<bool> WarningConfirmMessageBox(string title, string content) => await ConfirmMessageBox(title, content, Icon.Warning);
    public async Task<bool> WarningConfirmMessageBox(string content) => await WarningConfirmMessageBox(MsgBox_Title_Warning, content);

    public async Task<bool> FormatWarningConfirmMessageBox(string content, params object[] args) =>
        await WarningConfirmMessageBox(string.Format(content, args));

    #endregion
}