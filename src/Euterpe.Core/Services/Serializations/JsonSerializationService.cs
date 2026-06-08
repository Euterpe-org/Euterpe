using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static Euterpe.Core.JsonContexts.PascalCaseJsonContext;

namespace Euterpe.Core;

internal sealed class JsonSerializationService : IJsonSerializationService
{
    public async ValueTask<T> DeserializeFromFileAsync<T>(string filePath, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        var stream = File.OpenRead(filePath);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false)
                   ?? throw new InvalidDataException($"'{filePath}' is empty or invalid");
        }
    }

    public Config? DeserializeConfig(Stream utf8Json) =>
        JsonSerializer.Deserialize(utf8Json, Default.Config);

    public ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default) =>
        JsonSerializer.DeserializeAsync(utf8Json, Default.Config, cancellationToken);

    public void SerializeConfig(Stream utf8Json, Config value) =>
        JsonSerializer.Serialize(utf8Json, value, Default.Config);

    public Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default) =>
        JsonSerializer.SerializeAsync<Config>(utf8Json, value, Default.Config, cancellationToken);
}