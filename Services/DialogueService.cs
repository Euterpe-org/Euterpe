using System.Threading.Tasks;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services
{
    public class DialogueService : IDialogueService
    {
        public async Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.None)
            => await MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ButtonDefinitions = button,
                    ContentTitle = title,
                    ContentMessage = content,
                    Icon = icon
                }).Show();

        public async Task<ButtonResult> CreateErrorMessageBox(string title, string content) => await CreateMessageBox(title, content, icon: Icon.Error);
        public async Task<ButtonResult> CreateErrorMessageBox(string content) => await CreateErrorMessageBox("Failure", content);
    }
}
