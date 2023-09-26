using System.Text.Encodings.Web;
using System.Text.Json;
using MuseDashModToolsUI.Converters;

namespace MuseDashModToolsUI.Services;

public class SerializeService : ISerializeService
{
    public T? DeserializeFromJson<T>(string json, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(json, options);

    public Setting? DeserializeSetting(string json) => DeserializeFromJson<Setting>(
        json, new JsonSerializerOptions
        {
            Converters = { new SemanticVersionJsonConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });

    public string SerializeToJson(object obj, JsonSerializerOptions options) => JsonSerializer.Serialize(obj, options);

    public string SerializeSetting(Setting setting) => SerializeToJson(
        setting, new JsonSerializerOptions
        {
            Converters = { new SemanticVersionJsonConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });
}