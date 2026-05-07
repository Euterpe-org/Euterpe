namespace Euterpe.Generators.Tests.Generators;

public sealed class RouteGeneratorTests
{
    [Test]
    public Task Generates_route_tree_with_nested_children()
    {
        const string source = """
                              namespace Sample.Views
                              {
                                  using Euterpe.Shared.Attributes;

                                  [Route("/", DisplayName = "Home", Order = 0)]
                                  public partial class HomeView;

                                  [Route("/settings", DisplayName = "Settings", Icon = "gear", Order = 1)]
                                  public partial class SettingsView;

                                  [Route("/settings/general", DisplayName = "General", Order = 0)]
                                  public partial class GeneralSettingsView;

                                  [Route("/settings/advanced", DisplayName = "Advanced", Order = 1)]
                                  public partial class AdvancedSettingsView;
                              }

                              namespace Euterpe.Shared.Attributes
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                  public sealed class RouteAttribute(string path) : System.Attribute
                                  {
                                      public string Path { get; } = path;
                                      public string DisplayName { get; init; } = "";
                                      public string Icon { get; init; } = "";
                                      public int Order { get; init; }
                                  }
                              }
                              """;

        return Verify(GeneratorTestHelper.Run<RouteGenerator>(source));
    }
}