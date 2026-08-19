using Euterpe.CodeAnalysis.Analyzers;

namespace Euterpe.CodeAnalysis.Tests.Analyzers;

[TestSubject(typeof(RefitJsonContextAnalyzer))]
[Category("RefitJsonContextAnalyzerTests")]
public sealed class RefitJsonContextAnalyzerTest
{
    private const string SupportSource = """
                                                 using System;

                                                 namespace Refit
                                                 {
                                                     [AttributeUsage(AttributeTargets.Parameter)]
                                                     public sealed class BodyAttribute : Attribute;
                                                 }

                                                 namespace Sample
                                                 {
                                                     public sealed class Request;
                                                     public sealed class Response;
                                                 }
                                                 """;

    [Test]
    public async Task RegisteredReturnAndBodyTypes_NoDiagnostics()
    {
        var diagnostics = await RunAsync("""
                                         using System.Text.Json.Serialization;
                                         using System.Threading.Tasks;

                                         namespace Euterpe.Core.JsonContexts
                                         {
                                             [JsonSerializable(typeof(Sample.Request))]
                                             [JsonSerializable(typeof(Sample.Response))]
                                             internal sealed class SnakeCaseJsonContext;
                                         }

                                         namespace Euterpe.Core.Http.Clients
                                         {
                                             public interface IClient
                                             {
                                                 Task<Sample.Response> SendAsync([Refit.Body] Sample.Request request);
                                             }
                                         }
                                         """).ConfigureAwait(false);

        await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task UnregisteredReturnType_ReportsDiagnostic()
    {
        var diagnostics = await RunAsync("""
                                         namespace Euterpe.Core.JsonContexts
                                         {
                                             internal sealed class SnakeCaseJsonContext;
                                         }

                                         namespace Euterpe.Core.Http.Clients
                                         {
                                             public interface IClient
                                             {
                                                 System.Threading.Tasks.Task<Sample.Response> GetAsync();
                                             }
                                         }
                                         """).ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([RefitJsonContextAnalyzer.WireTypeRegistrationRuleId]);
    }

    [Test]
    public async Task UnregisteredBodyType_ReportsDiagnostic()
    {
        var diagnostics = await RunAsync("""
                                         namespace Euterpe.Core.JsonContexts
                                         {
                                             internal sealed class SnakeCaseJsonContext;
                                         }

                                         namespace Euterpe.Core.Http.Clients
                                         {
                                             public interface IClient
                                             {
                                                 System.Threading.Tasks.Task SendAsync([Refit.Body] Sample.Request request);
                                             }
                                         }
                                         """).ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([RefitJsonContextAnalyzer.WireTypeRegistrationRuleId]);
    }

    [Test]
    public async Task HttpResponseMessageReturnType_NoDiagnostics()
    {
        var diagnostics = await RunAsync("""
                                         namespace Euterpe.Core.JsonContexts
                                         {
                                             internal sealed class SnakeCaseJsonContext;
                                         }

                                         namespace Euterpe.Core.Http.Clients
                                         {
                                             public interface IClient
                                             {
                                                 System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> GetAsync();
                                             }
                                         }
                                         """).ConfigureAwait(false);

        await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task RegisteredArrayReturnType_NoDiagnostics()
    {
        var diagnostics = await RunAsync("""
                                         using System.Text.Json.Serialization;

                                         namespace Euterpe.Core.JsonContexts
                                         {
                                             [JsonSerializable(typeof(Sample.Response[]))]
                                             internal sealed class SnakeCaseJsonContext;
                                         }

                                         namespace Euterpe.Core.Http.Clients
                                         {
                                             public interface IClient
                                             {
                                                 System.Threading.Tasks.Task<Sample.Response[]> GetAsync();
                                             }
                                         }
                                         """).ConfigureAwait(false);

        await Assert.That(diagnostics).IsEmpty();
    }

    private static Task<ImmutableArray<Diagnostic>> RunAsync(string source) =>
        AnalyzerTestHelper.RunAsync<RefitJsonContextAnalyzer>("Euterpe.Core", SupportSource, source);
}
