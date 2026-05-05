using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Euterpe.Controls;

public sealed class EuterpeTheme : Styles
{
    public EuterpeTheme(IServiceProvider? sp = null) => AvaloniaXamlLoader.Load(sp, this);
}