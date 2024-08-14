using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Converters;

public sealed class SemanticVersionConverter : JsonConverter<SemanticVersion>
{
    public override SemanticVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        SemanticVersion.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, SemanticVersion semanticVersion, JsonSerializerOptions options) =>
        writer.WriteStringValue(semanticVersion.ToString());
}