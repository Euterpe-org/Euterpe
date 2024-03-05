using System.Text.Encodings.Web;
using System.Text.Json;
using MuseDashModToolsUI.Converters.JsonConverters;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public sealed class SerializationService : ISerializationService
{
    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public Lazy<ISavingService> SavingService { get; init; }

    public T? DeserializeFromJson<T>(string json, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(json, options);
    public T? DeserializeFromJson<T>(string json) => JsonSerializer.Deserialize<T>(json);

    public Setting? DeserializeSetting(string json) => DeserializeFromJson<Setting>(
        json, new JsonSerializerOptions
        {
            Converters = { new SemanticVersionJsonConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });

    public async Task<List<Mod>?> DeserializeModListAsync()
    {
        try
        {
            var mods = DeserializeFromJson<List<Mod>>(await File.ReadAllTextAsync(SavingService.Value.ModLinksPath));
            Logger.Information("Mod list deserialized");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Mod list deserialize failed");
            return null;
        }
    }

    public string SerializeToJson(object obj, JsonSerializerOptions options) => JsonSerializer.Serialize(obj, options);
    public string SerializeToJson(object obj) => JsonSerializer.Serialize(obj);

    public string SerializeSetting(Setting setting) => SerializeToJson(
        setting, new JsonSerializerOptions
        {
            Converters = { new SemanticVersionJsonConverter() },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });
}