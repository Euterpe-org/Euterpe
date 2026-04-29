using System.Text.Json;
using static Euterpe.Core.JsonContexts.PascalCaseJsonContext;

namespace Euterpe.Core;

internal sealed class JsonSerializationService : IJsonSerializationService
{
    public Config? DeserializeConfig(Stream utf8Json) =>
        JsonSerializer.Deserialize(utf8Json, Default.Config);

    public ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default) =>
        JsonSerializer.DeserializeAsync(utf8Json, Default.Config, cancellationToken);

    public void SerializeConfig(Stream utf8Json, Config value) =>
        JsonSerializer.Serialize(utf8Json, value, Default.Config);

    public Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default) =>
        JsonSerializer.SerializeAsync<Config>(utf8Json, value, Default.Config, cancellationToken);
}