using System.Text.Json;

namespace MuseDashModTools.Core;

internal sealed class JsonSerializationService : IJsonSerializationService
{
    #region AOT Compatible

    public ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default) =>
        JsonSerializer.DeserializeAsync(utf8Json, Default.Config, cancellationToken);

    public Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default) =>
        JsonSerializer.SerializeAsync<Config>(utf8Json, value, Default.Config, cancellationToken);

    #endregion AOT Compatible

    #region AOT Incompatible

    private static readonly JsonSerializerOptions IndentedSerializerOptions = new()
    {
        WriteIndented = true,
        IndentCharacter = ' ',
        IndentSize = 4
    };

#pragma warning disable IL2026, IL3050

    public T? Deserialize<T>(string text) => JsonSerializer.Deserialize<T>(text);

    public T? Deserialize<T>(string json, JsonSerializerOptions? options) =>
        JsonSerializer.Deserialize<T>(json, options);

    public T? DeserializeIndented<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, IndentedSerializerOptions);

    public ValueTask<T?> DeserializeAsync<T>(
        Stream utf8Json,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<T>(utf8Json, options, cancellationToken);

    public ValueTask<T?> DeserializeIndentedAsync<T>(Stream utf8Json, CancellationToken cancellationToken = default) =>
        JsonSerializer.DeserializeAsync<T>(utf8Json, IndentedSerializerOptions, cancellationToken);

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);

    public string Serialize<T>(T value, JsonSerializerOptions? options) =>
        JsonSerializer.Serialize(value, options);

    public string SerializeIndented<T>(T value) =>
        JsonSerializer.Serialize(value, IndentedSerializerOptions);

    public Task SerializeAsync<T>(
        Stream utf8Json,
        T value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
        => JsonSerializer.SerializeAsync(utf8Json, value, options, cancellationToken);

    public Task SerializeIndentedAsync<T>(
        Stream utf8Json,
        T value,
        CancellationToken cancellationToken = default)
        => JsonSerializer.SerializeAsync(utf8Json, value, IndentedSerializerOptions, cancellationToken);

#pragma warning restore IL3050, IL2026

    #endregion AOT Incompatible
}