using System.Text.Json.Serialization;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(SessionEvent))]
[JsonSerializable(typeof(ModDownloadEvent))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;