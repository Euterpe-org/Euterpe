using Avalonia.Markup.Xaml;

namespace Euterpe.Styles;

public sealed class EuterpeTheme : Avalonia.Styling.Styles
{
    public EuterpeTheme(IServiceProvider? sp = null) => AvaloniaXamlLoader.Load(sp, this);
}