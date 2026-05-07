using System.Text;
using Euterpe.Models.VDFs;

namespace Euterpe.Tests;

[Category("VdfSerializationServiceTests")]
[TestSubject(typeof(VdfSerializationService))]
public sealed class VdfSerializationServiceTest
{
    private readonly VdfSerializationService _service = new();

    [Test]
    public async Task DeserializeFromStream_KnownVdf_ParsesAllFields()
    {
        const string vdf = """
                           "library"
                           {
                               "path"      "C:/SteamLibrary"
                               "label"     "Main"
                               "contentid" "12345"
                               "totalsize" "999"
                               "apps"
                               {
                                   "774171"  "1024"
                                   "1228610" "2048"
                               }
                           }
                           """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(vdf));

        var result = _service.DeserializeFromStream<LibraryFolder>(stream);

        using var _ = Assert.Multiple();
        await Assert.That(result.Path).IsEqualTo("C:/SteamLibrary");
        await Assert.That(result.Label).IsEqualTo("Main");
        await Assert.That(result.ContentId).IsEqualTo("12345");
        await Assert.That(result.TotalSize).IsEqualTo("999");
        await Assert.That(result.Apps.Count).IsEqualTo(2);
        await Assert.That(result.Apps["774171"]).IsEqualTo("1024");
        await Assert.That(result.Apps["1228610"]).IsEqualTo("2048");
    }

    [Test]
    public async Task SerializeToStream_RoundTrip_PreservesAllFields()
    {
        var original = new LibraryFolder
        {
            Path = "C:/SteamLibrary",
            Label = "Main",
            ContentId = "12345",
            TotalSize = "999",
            Apps = new Dictionary<string, string>
            {
                ["774171"] = "1024",
                ["1228610"] = "2048"
            }
        };
        await using var stream = new MemoryStream();

        _service.SerializeToStream(stream, original, "library");
        stream.Position = 0;
        var result = _service.DeserializeFromStream<LibraryFolder>(stream);

        using var _ = Assert.Multiple();
        await Assert.That(result.Path).IsEqualTo(original.Path);
        await Assert.That(result.Label).IsEqualTo(original.Label);
        await Assert.That(result.ContentId).IsEqualTo(original.ContentId);
        await Assert.That(result.TotalSize).IsEqualTo(original.TotalSize);
        await Assert.That(result.Apps).IsEquivalentTo(original.Apps, KeyValuePairComparer<string, string>.Default);
    }

    [Test]
    public async Task SerializeToFile_AndDeserializeFromFile_RoundTrip_PreservesAllFields()
    {
        var original = new LibraryFolder
        {
            Path = "/home/user/SteamLibrary",
            Label = "Linux",
            ContentId = "67890",
            TotalSize = "12345",
            Apps = new Dictionary<string, string>
            {
                ["774171"] = "1024"
            }
        };

        var tempFile = Path.Combine(Path.GetTempPath(), $"vdf_{Guid.NewGuid():N}.vdf");
        try
        {
            _service.SerializeToFile(tempFile, original, "library");
            var roundTripped = _service.DeserializeFromFile<LibraryFolder>(tempFile);

            using var _ = Assert.Multiple();
            await Assert.That(roundTripped.Path).IsEqualTo(original.Path);
            await Assert.That(roundTripped.Label).IsEqualTo(original.Label);
            await Assert.That(roundTripped.ContentId).IsEqualTo(original.ContentId);
            await Assert.That(roundTripped.TotalSize).IsEqualTo(original.TotalSize);
            await Assert.That(roundTripped.Apps).IsEquivalentTo(original.Apps, KeyValuePairComparer<string, string>.Default);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Test]
    public async Task DeserializeFromFile_FileExists_ReadsContent()
    {
        const string vdf = """
                           "library"
                           {
                               "path"      "/file/path"
                               "label"     "FromFile"
                               "contentid" "999"
                               "totalsize" "1"
                               "apps"
                               {
                                   "1" "2"
                               }
                           }
                           """;
        var tempFile = Path.Combine(Path.GetTempPath(), $"vdf_{Guid.NewGuid():N}.vdf");
        try
        {
            await File.WriteAllTextAsync(tempFile, vdf);
            var result = _service.DeserializeFromFile<LibraryFolder>(tempFile);

            using var _ = Assert.Multiple();
            await Assert.That(result.Path).IsEqualTo("/file/path");
            await Assert.That(result.Label).IsEqualTo("FromFile");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}