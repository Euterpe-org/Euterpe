using System;

namespace MuseDashModToolsUI.Contracts;

public interface IDialogService
{
    void ShowDialog(object viewModel);
    void ShowDialog<T>();
    void ShowDialog(object viewModel, EventHandler openedEventHandler);
    void ShowDialog<T>(EventHandler openedEventHandler);
    void CloseDialog();
    void CloseDialog(EventHandler closedEventHandler);
}