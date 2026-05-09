namespace Euterpe.Generators.Tests.Generators;

public sealed class ServiceExtensionsGeneratorTests
{
    [Test]
    public Task Generates_registrations_for_app_and_per_game_views()
    {
        const string source = """
                              namespace Sample.Views;

                              public partial class HomeView : UserControl;

                              public partial class SettingsView : UserControl;

                              [PerGameView]
                              public partial class GameDetailView : UserControl;

                              [PerGameView]
                              public partial class GameSettingsView : UserControl;
                              """;

        return Verify(GeneratorTestHelper.Run<ServiceExtensionsGenerator>(source));
    }
}