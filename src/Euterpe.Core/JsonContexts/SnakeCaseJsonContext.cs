using System.Text.Json.Serialization;
using Euterpe.Models.Statistics;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease[]))]
[JsonSerializable(typeof(RecordVisitorRequest))]
[JsonSerializable(typeof(RecordDownloadRequest))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;