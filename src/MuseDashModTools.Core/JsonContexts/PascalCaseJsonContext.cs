using System.Text.Json.Serialization;
using MuseDashModTools.Core.Converters;

namespace MuseDashModTools.Core.JsonContexts;

[JsonSourceGenerationOptions(WriteIndented = true, IndentCharacter = ' ', IndentSize = 4, Converters = [typeof(SemVersionConverter)])]
[JsonSerializable(typeof(Config))]
internal sealed partial class PascalCaseJsonContext : JsonSerializerContext;