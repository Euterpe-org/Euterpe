using System.Text.Json;
using MuseDashModToolsUI.Contracts.Serializations;

namespace MuseDashModToolsUI.Services;

public sealed class JsonSerializationService : IJsonSerializationService
{
    public T? Deserialize<T>(string text) => JsonSerializer.Deserialize<T>(text);

    public T? Deserialize<T>(string json, JsonSerializerOptions? options) =>
        JsonSerializer.Deserialize<T>(json, options);

    public ValueTask<T?> DeserializeAsync<T>(
        Stream utf8Json,
        JsonSerializerOptions? options = default,
        CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<T>(utf8Json, options, cancellationToken);

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);

    public string Serialize<T>(T value, JsonSerializerOptions? options) =>
        JsonSerializer.Serialize(value, options);

    public Task SerializeAsync<T>(
        Stream utf8Json,
        T value,
        JsonSerializerOptions? options = default,
        CancellationToken cancellationToken = default)
        => JsonSerializer.SerializeAsync(utf8Json, value, options, cancellationToken);
}