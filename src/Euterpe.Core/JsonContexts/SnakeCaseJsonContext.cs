using System.Text.Json.Serialization;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease[]))]
[JsonSerializable(typeof(VisitorTelemetryRequest))]
[JsonSerializable(typeof(DownloadTelemetryRequest))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;