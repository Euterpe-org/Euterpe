using System.Text.Json;
using Euterpe.Contracts.GitHub;

namespace Euterpe.Tests.Contracts;

[Category("GitHubContractsTests")]
[TestSubject(typeof(GitHubRelease))]
public sealed class GitHubContractsTest
{
    private static readonly JsonSerializerOptions SnakeCase = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    [Test]
    public async Task GitHubRelease_Defaults_AreEmptyStringsAndZero()
    {
        var release = new GitHubRelease();

        using var _ = Assert.Multiple();
        await Assert.That(release.Url).IsEqualTo(string.Empty);
        await Assert.That(release.AssetsUrl).IsEqualTo(string.Empty);
        await Assert.That(release.UploadUrl).IsEqualTo(string.Empty);
        await Assert.That(release.HtmlUrl).IsEqualTo(string.Empty);
        await Assert.That(release.Id).IsEqualTo(0);
        await Assert.That(release.NodeId).IsEqualTo(string.Empty);
        await Assert.That(release.TagName).IsEqualTo(string.Empty);
        await Assert.That(release.TargetCommitish).IsEqualTo(string.Empty);
        await Assert.That(release.Name).IsEqualTo(string.Empty);
        await Assert.That(release.Draft).IsFalse();
        await Assert.That(release.Prerelease).IsFalse();
        await Assert.That(release.Assets).IsEmpty();
        await Assert.That(release.TarballUrl).IsEqualTo(string.Empty);
        await Assert.That(release.ZipballUrl).IsEqualTo(string.Empty);
        await Assert.That(release.Body).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task GitHubRelease_RoundTripJson_PreservesValues()
    {
        var release = new GitHubRelease
        {
            Url = "https://api.github.com/repos/x/y/releases/1",
            Id = 1,
            TagName = "v1.0.0",
            Name = "1.0.0",
            Draft = false,
            Prerelease = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PublishedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            Assets = [new GitHubReleaseAsset { Id = 99, Name = "asset.zip", Size = 1024 }],
            Body = "release notes"
        };

        var json = JsonSerializer.Serialize(release, SnakeCase);
        var parsed = JsonSerializer.Deserialize<GitHubRelease>(json, SnakeCase)!;

        using var _ = Assert.Multiple();
        await Assert.That(parsed.Id).IsEqualTo(release.Id);
        await Assert.That(parsed.TagName).IsEqualTo(release.TagName);
        await Assert.That(parsed.Prerelease).IsEqualTo(release.Prerelease);
        await Assert.That(parsed.Assets.Length).IsEqualTo(1);
        await Assert.That(parsed.Assets[0].Name).IsEqualTo("asset.zip");
        await Assert.That(parsed.Body).IsEqualTo(release.Body);
    }

    [Test]
    public async Task GitHubReleaseAsset_Defaults_AreEmptyAndZero()
    {
        var asset = new GitHubReleaseAsset();

        using var _ = Assert.Multiple();
        await Assert.That(asset.Url).IsEqualTo(string.Empty);
        await Assert.That(asset.Id).IsEqualTo(0);
        await Assert.That(asset.NodeId).IsEqualTo(string.Empty);
        await Assert.That(asset.Name).IsEqualTo(string.Empty);
        await Assert.That(asset.Label).IsNull();
        await Assert.That(asset.ContentType).IsEqualTo(string.Empty);
        await Assert.That(asset.State).IsEqualTo(string.Empty);
        await Assert.That(asset.Size).IsEqualTo(0);
        await Assert.That(asset.DownloadCount).IsEqualTo(0);
        await Assert.That(asset.BrowserDownloadUrl).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task GitHubRepoContent_Defaults_AreEmptyAndZeroAndLinksInitialized()
    {
        var content = new GitHubRepoContent();

        using var _ = Assert.Multiple();
        await Assert.That(content.Name).IsEqualTo(string.Empty);
        await Assert.That(content.Path).IsEqualTo(string.Empty);
        await Assert.That(content.SHA).IsEqualTo(string.Empty);
        await Assert.That(content.Size).IsEqualTo(0);
        await Assert.That(content.Url).IsEqualTo(string.Empty);
        await Assert.That(content.HtmlUrl).IsEqualTo(string.Empty);
        await Assert.That(content.GitUrl).IsEqualTo(string.Empty);
        await Assert.That(content.DownloadUrl).IsEqualTo(string.Empty);
        await Assert.That(content.Type).IsEqualTo(string.Empty);
        await Assert.That(content.Links).IsNotNull();
    }

    [Test]
    public async Task GitHubRepoContent_RoundTripJson_RespectsRenamedProperties()
    {
        const string json = """
                            {
                              "name": "README.md",
                              "path": "README.md",
                              "sha": "abc123",
                              "size": 100,
                              "url": "https://api.example/u",
                              "html_url": "https://example/h",
                              "git_url": "https://example/g",
                              "download_url": "https://example/d",
                              "type": "file",
                              "_links": { "self": "s", "git": "g", "html": "h" }
                            }
                            """;
        var parsed = JsonSerializer.Deserialize<GitHubRepoContent>(json, SnakeCase)!;

        using var _ = Assert.Multiple();
        await Assert.That(parsed.SHA).IsEqualTo("abc123");
        await Assert.That(parsed.Links.Self).IsEqualTo("s");
        await Assert.That(parsed.Links.Git).IsEqualTo("g");
        await Assert.That(parsed.Links.Html).IsEqualTo("h");
    }

    [Test]
    public async Task GitHubReadmeContent_Defaults_InheritFromRepoContent()
    {
        var readme = new GitHubReadmeContent();

        using var _ = Assert.Multiple();
        await Assert.That(readme.Content).IsEqualTo(string.Empty);
        await Assert.That(readme.Encoding).IsEqualTo(string.Empty);
        await Assert.That(readme.Name).IsEqualTo(string.Empty);
        await Assert.That(readme.Links).IsNotNull();
    }

    [Test]
    public async Task GitHubRepoContentLinks_Defaults_AreEmpty()
    {
        var links = new GitHubRepoContentLinks();

        using var _ = Assert.Multiple();
        await Assert.That(links.Self).IsEqualTo(string.Empty);
        await Assert.That(links.Git).IsEqualTo(string.Empty);
        await Assert.That(links.Html).IsEqualTo(string.Empty);
    }
}
