using System;

namespace MuseDashModToolsUI.Contracts;

public interface IDialogService
{
    void ShowDialog(object data);
    void ShowDialog<T>();
    void ShowDialog(object data, EventHandler openedEventHandler);
    void ShowDialog<T>(EventHandler openedEventHandler);
    void CloseDialog();
}