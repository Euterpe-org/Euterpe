namespace Euterpe.Generators.Tests.Generators;

[TestSubject(typeof(ServiceExtensionsGenerator))]
[Category("ServiceExtensionsGeneratorTests")]
public sealed class ServiceExtensionsGeneratorTest
{
    [Test]
    public Task Generates_registrations_for_app_and_per_game_view_models()
    {
        const string source = """
                              namespace Sample
                              {
                                  using Euterpe.Shared.Attributes;

                                  [Route("/")]
                                  public partial class RootViewModel;

                                  [Route("/home")]
                                  public partial class HomeViewModel;

                                  [Route("/modding")]
                                  [PerGame]
                                  public partial class ModdingViewModel;

                                  [PerGame]
                                  public partial class WizardDialogViewModel;

                                  [Register]
                                  public partial class CrashViewModel;
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

                                  [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                  public sealed class PerGameAttribute : System.Attribute;

                                  [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                  public sealed class RegisterAttribute : System.Attribute;
                              }
                              """;

        return Verify(GeneratorTestHelper.Run<ServiceExtensionsGenerator>(source));
    }
}
