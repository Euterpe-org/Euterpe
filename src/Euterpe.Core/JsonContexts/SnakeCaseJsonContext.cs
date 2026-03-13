using System.Text.Json.Serialization;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease[]))]
[JsonSerializable(typeof(VisitorEvent))]
[JsonSerializable(typeof(ModDownloadEvent))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;