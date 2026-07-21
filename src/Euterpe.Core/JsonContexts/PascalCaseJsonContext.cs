using System.Text.Json.Serialization;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(WriteIndented = true, IndentCharacter = ' ', IndentSize = 4)]
[JsonSerializable(typeof(Config))]
internal sealed partial class PascalCaseJsonContext : JsonSerializerContext;
