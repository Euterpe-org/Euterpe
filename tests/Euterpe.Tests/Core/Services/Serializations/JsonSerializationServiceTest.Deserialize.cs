using System.Text;

namespace Euterpe.Tests;

public sealed partial class JsonSerializationServiceTest
{
    [Test]
    public Task DeserializeConfig_ShouldReturnValidConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = _jsonSerializationService.DeserializeConfig(stream);

        return Verify(config);
    }

    [Test]
    public async Task DeserializeConfigAsync_ShouldReturnValidConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = await _jsonSerializationService.DeserializeConfigAsync(stream);

        await Verify(config);
    }
}