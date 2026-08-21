using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeModClientTests")]
[TestSubject(typeof(IEuterpeModClient))]
public sealed class IEuterpeModClientTest
{
    private const string GoldenManifestJson =
        """[{"mid":1,"name":"CustomAlbums","version":"4.1.0","author":"A","file_name":"CustomAlbums.dll","repository":"r","config_file":"c","game_version":"4.0.0","melon_version":"0.6.1","description":"d","mod_dependencies":["Lib"],"lib_dependencies":[],"incompatible_mods":[],"screenshots":[],"sha256":"abc123","download_url":"https://dl/x","download_count_total":42}]""";

    [Test]
    public async Task GetModManifestAsync_GoldenServerJson_MapsOverriddenAndPolicyNamedFields()
    {
        using var http = Mock.HttpHandler();
        http.OnGet("/api/mods/app-manifest").RespondWithJson(GoldenManifestJson);
        var api = http.CreateEuterpeClient<IEuterpeModClient>(Mods.BasePath);

        var mods = await api.GetModManifestAsync();

        var mod = mods.Single();
        using var assertions = Assert.Multiple();
        await Assert.That(mod.FileName).IsEqualTo("CustomAlbums.dll");
        await Assert.That(mod.MelonVersion).IsEqualTo("0.6.1");
        await Assert.That(mod.SHA256).IsEqualTo("abc123");
        await Assert.That(mod.DownloadCount).IsEqualTo(42);
        await Assert.That(mod.ModDependencies)
            .IsEquivalentTo(["Lib"], StringComparer.Ordinal, CollectionOrdering.Matching);
    }
}
