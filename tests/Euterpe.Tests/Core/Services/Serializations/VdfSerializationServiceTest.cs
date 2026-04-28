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
}