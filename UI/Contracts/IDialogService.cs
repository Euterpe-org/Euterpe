namespace MuseDashModToolsUI.Contracts;

public interface IDialogService
{
    void ShowDialog(object data);
    void ShowDialog<T>();
    void CloseDialog();
}