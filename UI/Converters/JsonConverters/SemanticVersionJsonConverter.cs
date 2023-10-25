using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Converters.JsonConverters;

public class SemanticVersionJsonConverter : JsonConverter<SemanticVersion>
{
    public override SemanticVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var versionString = reader.GetString();
        return SemanticVersion.Parse(versionString!);
    }

    public override void Write(Utf8JsonWriter writer, SemanticVersion semanticVersion, JsonSerializerOptions options)
    {
        var releaseLabel = semanticVersion.ReleaseLabels.FirstOrDefault();
        var versionString = string.IsNullOrEmpty(releaseLabel)
            ? $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}"
            : $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}-{releaseLabel}";
        writer.WriteStringValue(versionString);
    }
}