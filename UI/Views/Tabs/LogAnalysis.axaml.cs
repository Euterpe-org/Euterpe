using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MuseDashModToolsUI.Views.Tabs;

public partial class LogAnalysis : UserControl
{
    public LogAnalysis()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}