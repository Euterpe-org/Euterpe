using System.IO;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MuseDashModToolsUI.Views.Controls;

namespace MuseDashModToolsUI.Views.Tabs;

public partial class InfoJson : UserControl
{
    public IMessageBoxService MessageBoxService { get; init; }
    private List<MapInfoEditor> _editors = new List<MapInfoEditor>();

    public InfoJson()
    {
        InitializeComponent();
        init();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        init();
    }

    private async Task init()
    {
        string? path = await openSelect();
        bool found = false;
        for (int i = 1; i < 4; i++)
        {
            string file = $"map{i}.bms";
            if (File.Exists(Path.Combine(path, file)))
            {
                if (!found)
                    _editors.Clear();
                found = true;
                var editor = new MapInfoEditor();
                TabControl.Items.Add(new TabItem { Header = file, Content = editor } );
                _editors.Add(editor);
            }
        }
        if (found)
            PathTextBox.Text = path;
        else
        {
            await MessageBoxService.ErrorMessageBox("没有找到谱面");
            init();
        }
    }

    private async Task<string?> openSelect()
    {
        var dialog = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false, Title = FolderDialog_Title });
        return dialog.FirstOrDefault()?.TryGetLocalPath();
    }
}