using System.Text.Json.Serialization.Metadata;

namespace Euterpe.Abstractions;

public interface IJsonSerializationService
{
    ValueTask<T> DeserializeFromFileAsync<T>(string filePath, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default);
    Config? DeserializeConfig(Stream utf8Json);
    ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default);
    void SerializeConfig(Stream utf8Json, Config value);
    Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default);
}