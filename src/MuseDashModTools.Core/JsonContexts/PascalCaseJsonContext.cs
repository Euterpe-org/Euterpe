using System.Text.Json.Serialization;

namespace MuseDashModTools.Core.JsonContexts;

[JsonSourceGenerationOptions(WriteIndented = true, IndentCharacter = ' ', IndentSize = 4)]
[JsonSerializable(typeof(Config))]
internal sealed partial class PascalCaseJsonContext : JsonSerializerContext;