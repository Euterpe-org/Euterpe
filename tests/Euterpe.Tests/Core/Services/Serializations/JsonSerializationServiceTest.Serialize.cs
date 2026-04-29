namespace Euterpe.Tests;

public sealed partial class JsonSerializationServiceTest
{
    [Test]
    public Task SerializeConfig_ShouldReturnValidJson()
    {
        var stream = new MemoryStream();
        _jsonSerializationService.SerializeConfig(stream, CreateTestConfig());
        stream.Position = 0;
        return VerifyJson(stream);
    }

    [Test]
    public async Task SerializeConfigAsync_ShouldReturnValidJson()
    {
        var stream = new MemoryStream();
        await _jsonSerializationService.SerializeConfigAsync(stream, CreateTestConfig());
        stream.Position = 0;
        await VerifyJson(stream);
    }
}