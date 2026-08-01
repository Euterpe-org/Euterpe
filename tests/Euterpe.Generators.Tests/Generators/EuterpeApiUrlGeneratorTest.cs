namespace Euterpe.Generators.Tests.Generators;

[TestSubject(typeof(EuterpeApiUrlGenerator))]
[Category("EuterpeApiUrlGeneratorTests")]
public sealed class EuterpeApiUrlGeneratorTest
{
    [Test]
    public Task Generates_urls_from_euterpe_api()
    {
        const string source = """
                              namespace Euterpe.Shared
                              {
                                  public static partial class EuterpeApi
                                  {
                                      public const string BaseUrl = "https://example.com/api/";

                                      public static partial class Account
                                      {
                                          public const string BasePath = "me";
                                      }

                                      public static partial class Auth
                                      {
                                          public const string BasePath = "auth";
                                          public const string AppToken = "/app/token";
                                          public const string Logout = "/logout";
                                      }

                                      public static partial class Distribution
                                      {
                                          public const string BasePath = "distribution";
                                          public const string LibsPath = "/libs";
                                      }
                                  }
                              }
                              """;

        return Verify(GeneratorTestHelper.Run<EuterpeApiUrlGenerator>(source));
    }
}
