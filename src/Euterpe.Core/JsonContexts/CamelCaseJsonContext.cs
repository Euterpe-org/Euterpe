using System.Text.Json.Serialization;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Mod[]))]
[JsonSerializable(typeof(Lib[]))]
[JsonSerializable(typeof(Chart[]))]
internal sealed partial class CamelCaseJsonContext : JsonSerializerContext;