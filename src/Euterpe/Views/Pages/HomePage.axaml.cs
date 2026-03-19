using Avalonia.Interactivity;

namespace Euterpe.Views.Pages;

[Route("/home", DisplayName = Page_Home, Icon = "Home", Order = 0)]
public sealed partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        SupportCard.IsVisible = false;
    }

    private void TitleButton_Click(object? sender, RoutedEventArgs e)
    {
        // Just for visual effect, no actual action
    }
}