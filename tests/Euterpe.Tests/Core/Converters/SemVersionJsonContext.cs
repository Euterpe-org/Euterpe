using System.Text.Json.Serialization;
using Euterpe.Core.Converters;
using Semver;

namespace Euterpe.Tests.Core.Converters;

[JsonSourceGenerationOptions(Converters = [typeof(SemVersionConverter)])]
[JsonSerializable(typeof(SemVersion))]
internal sealed partial class SemVersionJsonContext : JsonSerializerContext;
