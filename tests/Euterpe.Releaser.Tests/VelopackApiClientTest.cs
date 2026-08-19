using System.Net;
using Euterpe.Releaser;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using TUnit.Mocks.Http;

namespace Euterpe.Releaser.Tests;

[Category("ReleaserTests")]
[TestSubject(typeof(VelopackApiClient))]
public sealed class VelopackApiClientTest
{
    [Test]
    public async Task UploadAssetAsync_TransientFailure_RetriesPutRequest()
    {
        const string requestPath =
            "/api/workspace/velopack/linux-x64-beta/2.1.0-beta.2/asset/Euterpe-2.1.0-beta.2-linux-x64-beta-full.nupkg";
        byte[] expectedContent = [1, 2, 3, 4];
        var primary = Mock.HttpHandler();
        var upload = primary.OnPut(requestPath);
        upload.Respond((HttpStatusCode)554);
        upload.Respond(HttpStatusCode.NoContent);
        var assetDirectory = Path.Combine(Path.GetTempPath(), $"euterpe-releaser-{Guid.NewGuid():N}");
        var assetPath = Path.Combine(assetDirectory, "Euterpe-2.1.0-beta.2-linux-x64-beta-full.nupkg");

        try
        {
            Directory.CreateDirectory(assetDirectory);
            await File.WriteAllBytesAsync(assetPath, expectedContent);
            await using var provider = CreateProvider(primary);
            var client = provider.GetRequiredService<VelopackApiClient>();

            await client.UploadAssetAsync(
                "linux-x64-beta",
                SemVersion.Parse("2.1.0-beta.2", SemVersionStyles.Strict),
                "full",
                assetPath,
                CancellationToken.None);

            using var assertions = Assert.Multiple();
            await Assert.That(primary.Requests.Count).IsEqualTo(2);
            await Assert.That(primary.Requests.All(request => request.Method == HttpMethod.Put)).IsTrue();
            await Assert.That(primary.Requests.All(request => request.RequestUri!.AbsolutePath == requestPath)).IsTrue();
        }
        finally
        {
            File.Delete(assetPath);
            Directory.Delete(assetDirectory);
        }
    }

    [Test]
    public async Task PublishAsync_TransientFailure_DoesNotRetryPostRequest()
    {
        const string requestPath = "/api/workspace/velopack/publish";
        var primary = Mock.HttpHandler();
        primary.OnPost(requestPath).Respond((HttpStatusCode)554);
        await using var provider = CreateProvider(primary);
        var client = provider.GetRequiredService<VelopackApiClient>();

        Func<Task> act = () => client.PublishAsync(
            SemVersion.Parse("2.1.0-beta.2", SemVersionStyles.Strict),
            CancellationToken.None);

        await Assert.That(act).Throws<HttpRequestException>();
        await Assert.That(primary.Requests).HasSingleItem();
        await Assert.That(primary.Requests[0].RequestUri!.AbsolutePath).IsEqualTo(requestPath);
    }

    private static ServiceProvider CreateProvider(MockHttpHandler primary)
    {
        var services = new ServiceCollection();
        services.RegisterReleaserServices();
        services.AddHttpClient<VelopackApiClient>().ConfigurePrimaryHttpMessageHandler(() => primary);
        return services.BuildServiceProvider();
    }
}
