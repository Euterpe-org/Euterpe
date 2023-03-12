using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services;

public class DialogueService : IDialogueService
{
    public async Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Success)
    {
        var desktop = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var isMainWindow = desktop?.MainWindow is not null;
        var messageBox = MessageBoxManager
            .GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = button,
                ContentTitle = title,
                ContentMessage = content,
                Icon = icon,
                Topmost = true,
                WindowStartupLocation = isMainWindow ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen
            });
        return isMainWindow ? await messageBox.Show(desktop!.MainWindow) : await messageBox.Show();
    }


    public async Task<ButtonResult> CreateErrorMessageBox(string title, string content) => await CreateMessageBox(title, content, icon: Icon.Error);
    public async Task<ButtonResult> CreateErrorMessageBox(string content) => await CreateErrorMessageBox("Failure", content);

    public async Task<bool> CreateConfirmMessageBox(string title, string content)
    {
        var result = await CreateMessageBox(title, content, ButtonEnum.YesNo, Icon.Warning);
        return result.HasFlag(ButtonResult.Yes) && !result.HasFlag(ButtonResult.None);
    }

    public async Task<bool> CreateConfirmMessageBox(string content) => await CreateConfirmMessageBox("Warning", content);
}