using System.Text.Json.Serialization;

namespace MuseDashModTools.Core;

[JsonSourceGenerationOptions(WriteIndented = true, IndentCharacter = ' ', IndentSize = 4)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(Mod))]
[JsonSerializable(typeof(Lib))]
[JsonSerializable(typeof(Chart))]
[JsonSerializable(typeof(GitHubRelease[]))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext;