using System.Text.Json.Serialization;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(InfoJson))]
internal sealed partial class CamelCaseContext : JsonSerializerContext;