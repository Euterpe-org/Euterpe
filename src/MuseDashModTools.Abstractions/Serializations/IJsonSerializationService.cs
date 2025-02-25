using System.Text.Json;

namespace MuseDashModTools.Abstractions;

public interface IJsonSerializationService : ISerializationService
{
    #region AOT Compatible

    ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default);
    Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default);

    #endregion AOT Compatible

    #region AOT Incompatible

    T? Deserialize<T>(string json, JsonSerializerOptions? options);
    T? DeserializeIndented<T>(string json);

    ValueTask<T?> DeserializeAsync<T>(
        Stream utf8Json,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<T?> DeserializeIndentedAsync<T>(
        Stream utf8Json,
        CancellationToken cancellationToken = default);

    string Serialize<T>(T value, JsonSerializerOptions? options);
    string SerializeIndented<T>(T value);

    Task SerializeAsync<T>(
        Stream utf8Json,
        T value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default);

    Task SerializeIndentedAsync<T>(
        Stream utf8Json,
        T value,
        CancellationToken cancellationToken = default);

    #endregion AOT Incompatible
}