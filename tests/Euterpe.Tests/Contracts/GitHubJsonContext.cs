using System.Text.Json.Serialization;
using Euterpe.Contracts.GitHub;

namespace Euterpe.Tests.Contracts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(GitHubRepoContent))]
internal sealed partial class GitHubJsonContext : JsonSerializerContext;
