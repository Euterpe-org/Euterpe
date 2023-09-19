using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MuseDashModToolsUI.Services;

public partial class MessageBoxService : IMessageBoxService
{
    public Lazy<ISavingService>? SavingService { get; init; }

    #region Custom Message Box

    public async Task<string> CustomConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info)
    {
        var buttonDefinitions = GetButtonDefinition(buttonCount);
        return await CustomMessageBox(title, content, buttonDefinitions, icon);
    }

    public async Task<string> CustomConfirmMessageBox(string content, int buttonCount) =>
        await CustomConfirmMessageBox(MsgBox_Title_Notice, content, buttonCount);

    public async Task<string> CustomMarkDownConfirmMessageBox(string title, string content, int buttonCount, Icon icon = Icon.Info)
    {
        var buttonDefinitions = buttonCount switch
        {
            3 => new[]
            {
                new ButtonDefinition { Name = MsgBox_Button_Yes, IsDefault = true },
                new ButtonDefinition { Name = MsgBox_Button_No, IsCancel = true },
                new ButtonDefinition { Name = MsgBox_Button_NoNoAsk, IsCancel = true }
            },
            4 => new[]
            {
                new ButtonDefinition { Name = MsgBox_Button_Yes, IsDefault = true },
                new ButtonDefinition { Name = MsgBox_Button_YesNoAsk, IsCancel = false },
                new ButtonDefinition { Name = MsgBox_Button_No, IsCancel = true },
                new ButtonDefinition { Name = MsgBox_Button_NoNoAsk, IsCancel = true }
            },
            _ => new[]
            {
                new ButtonDefinition { Name = MsgBox_Button_Yes, IsDefault = true },
                new ButtonDefinition { Name = MsgBox_Button_No, IsCancel = true }
            }
        };

        return await CustomMarkDownMessageBox(title, content, buttonDefinitions, icon);
    }

    public async Task<string> CustomMarkDownConfirmMessageBox(string content, int buttonCount) =>
        await CustomMarkDownConfirmMessageBox(MsgBox_Title_Notice, content, buttonCount);

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

    public async Task<ButtonResult> ErrorMessageBox(string content, Icon icon = Icon.Error) =>
        await MessageBox(MsgBox_Title_Failure, content, icon);

    public async Task<ButtonResult> ErrorMessageBox(string content, Exception ex) =>
        await ErrorMessageBox(string.Format(content, ex));

    public async Task<ButtonResult> NoticeMessageBox(string content, Icon icon = Icon.Info) =>
        await MessageBox(MsgBox_Title_Notice, content, icon);

    public async Task<ButtonResult> SuccessMessageBox(string content, Icon icon = Icon.Success) =>
        await MessageBox(MsgBox_Title_Success, content, icon);

    public async Task<ButtonResult> WarningMessageBox(string content, Icon icon = Icon.Warning) =>
        await MessageBox(MsgBox_Title_Warning, content, icon);

    public async Task<bool> ConfirmMessageBox(string title, string content, Icon icon)
    {
        var result = await CustomConfirmMessageBox(title, content, 2, icon);
        return result == MsgBox_Button_Yes;
    }

    public async Task<bool> NoticeConfirmMessageBox(string title, string content) => await ConfirmMessageBox(title, content, Icon.Info);
    public async Task<bool> NoticeConfirmMessageBox(string content) => await ConfirmMessageBox(MsgBox_Title_Notice, content, Icon.Info);
    public async Task<bool> WarningConfirmMessageBox(string title, string content) => await ConfirmMessageBox(title, content, Icon.Warning);
    public async Task<bool> WarningConfirmMessageBox(string content) => await WarningConfirmMessageBox(MsgBox_Title_Warning, content);

    #endregion
}