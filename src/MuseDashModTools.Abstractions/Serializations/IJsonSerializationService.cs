namespace MuseDashModTools.Abstractions;

public interface IJsonSerializationService
{
    ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default);
    Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default);
}