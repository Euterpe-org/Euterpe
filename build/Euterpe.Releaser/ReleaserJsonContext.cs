namespace Euterpe.Releaser;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(VelopackReleaseBase))]
[JsonSerializable(typeof(VelopackPublishRequest))]
internal sealed partial class ReleaserJsonContext : JsonSerializerContext;
