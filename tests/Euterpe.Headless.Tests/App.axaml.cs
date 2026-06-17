using Avalonia.Markup.Xaml;

namespace Euterpe.Headless.Tests;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
