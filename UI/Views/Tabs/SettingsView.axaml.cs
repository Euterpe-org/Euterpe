using Avalonia.Controls;
using MuseDashModToolsUI.Contracts.ViewModels;
using Splat;

namespace MuseDashModToolsUI.Views.Tabs;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        DataContext = Locator.Current.GetRequiredService<ISettingsViewModel>();
        InitializeComponent();
    }
}