using System.Threading.Tasks;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace MuseDashModToolsUI.Utils
{
    public static class MessageBoxUtils
    {
        public static async Task<ButtonResult> CreateMessageBox(string title, string content, ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.None)
            => await MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                {
                    ButtonDefinitions = button,
                    ContentTitle = title,
                    ContentMessage = content,
                    Icon = icon
                }).Show();

        public static async Task<ButtonResult> CreateErrorMessageBox(string title, string content) => await CreateMessageBox(title, content, icon: Icon.Error);
        public static async Task<ButtonResult> CreateErrorMessageBox(string content) => await CreateErrorMessageBox("Failure", content);
    }
}
