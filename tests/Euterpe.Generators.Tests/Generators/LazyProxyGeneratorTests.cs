namespace Euterpe.Generators.Tests.Generators;

public sealed class LazyProxyGeneratorTests
{
    [Test]
    public Task Generates_proxy_for_class_with_methods_and_properties()
    {
        const string source = """
                              namespace Sample
                              {
                                  using Euterpe.Shared.Attributes;

                                  public class Greeter
                                  {
                                      public virtual string Greet(string name) => $"Hello, {name}";
                                      public virtual int Count { get; set; }
                                  }

                                  [LazyProxy(typeof(Greeter))]
                                  public partial class GreeterProxy;
                              }

                              namespace Euterpe.Shared.Attributes
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Class)]
                                  public sealed class LazyProxyAttribute(System.Type baseType) : System.Attribute;
                              }
                              """;

        return Verify(GeneratorTestHelper.Run<LazyProxyGenerator>(source));
    }
}