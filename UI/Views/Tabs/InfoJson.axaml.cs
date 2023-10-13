namespace MuseDashModToolsUI.Views.Tabs;

public partial class InfoJson : UserControl
{
    public InfoJson()
    {
        InitializeComponent();
    }

    /*private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        init();
    }

    private async Task init()
    {
        var path = await openSelect();
        var found = false;
        for (var i = 1; i < 4; i++)
        {
            var file = $"map{i}.bms";
            if (File.Exists(Path.Combine(path, file)))
            {
                if (!found)
                    _editors.Clear();
                found = true;
                var editor = new MapInfoEditor();
                TabControl.Items.Add(new TabItem { Header = file, Content = editor });
                _editors.Add(editor);
            }
        }

        if (found)
        {
            PathTextBox.Text = path;
        }
        else
        {
            await MessageBoxService.ErrorMessageBox("没有找到谱面");
            init();
        }
    }

    private async Task<string?> openSelect()
    {
        var dialog = await new Window().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { AllowMultiple = false, Title = FolderDialog_Title });
        return dialog.FirstOrDefault()?.TryGetLocalPath();
    }*/
}