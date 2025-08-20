using System.Text.Json;
using static MuseDashModTools.Core.JsonContexts.PascalCaseJsonContext;

namespace MuseDashModTools.Core;

internal sealed class JsonSerializationService : IJsonSerializationService
{
    public ValueTask<Config?> DeserializeConfigAsync(Stream utf8Json, CancellationToken cancellationToken = default) =>
        JsonSerializer.DeserializeAsync(utf8Json, Default.Config, cancellationToken);

    public Task SerializeConfigAsync(Stream utf8Json, Config value, CancellationToken cancellationToken = default) =>
        JsonSerializer.SerializeAsync<Config>(utf8Json, value, Default.Config, cancellationToken);
}