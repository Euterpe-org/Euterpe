using System.Text.Json.Serialization;

namespace MuseDashModTools.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease[]))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;