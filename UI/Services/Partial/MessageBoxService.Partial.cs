using MsBox.Avalonia.Models;

namespace MuseDashModToolsUI.Services;

public sealed partial class MessageBoxService
{
    /// <summary>
    ///     Get button definition by button count
    /// </summary>
    /// <param name="buttonCount"></param>
    /// <returns></returns>
    private static ButtonDefinition[] GetButtonDefinition(int buttonCount)
    {
        var buttonDefinitions = buttonCount switch
        {
            3 =>
            [
                new ButtonDefinition { Name = MsgBox_Button_Yes, IsDefault = true },
                new ButtonDefinition { Name = MsgBox_Button_No, IsCancel = true },
                new ButtonDefinition { Name = MsgBox_Button_NoNoAsk, IsCancel = true }
            ],
            4 =>
            [
                new ButtonDefinition { Name = MsgBox_Button_Yes, IsDefault = true },
                new ButtonDefinition { Name = MsgBox_Button_YesNoAsk, IsCancel = false },
                new ButtonDefinition { Name = MsgBox_Button_No, IsCancel = true },
                new ButtonDefinition { Name = MsgBox_Button_NoNoAsk, IsCancel = true }
            ],
            _ => new[]
            {
                new ButtonDefinition { Name = MsgBox_Button_Yes, IsDefault = true },
                new ButtonDefinition { Name = MsgBox_Button_No, IsCancel = true }
            }
        };
        return buttonDefinitions;
    }
}