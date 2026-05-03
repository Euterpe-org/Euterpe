namespace Euterpe.Abstractions;

public interface IJsonSerializationService
{
    Config? DeserializeConfig(Stream utf8Json);
    ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default);
    void SerializeConfig(Stream utf8Json, Config value);
    Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default);
}