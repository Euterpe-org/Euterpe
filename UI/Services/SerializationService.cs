using System.Text.Json;
using MemoryPack;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public sealed class SerializationService : ISerializationService
{
    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    public T? DeserializeFromJson<T>(string json, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(json, options);
    public T? DeserializeFromJson<T>(string json) => JsonSerializer.Deserialize<T>(json);

    public Setting? DeserializeSetting(byte[] text) => MemoryPackSerializer.Deserialize<Setting>(text);
    public async ValueTask<Setting?> DeserializeSettingAsync(Stream stream) => await MemoryPackSerializer.DeserializeAsync<Setting>(stream);

    public async Task<List<Mod>?> DeserializeModListAsync()
    {
        try
        {
            var mods = DeserializeFromJson<List<Mod>>(await File.ReadAllTextAsync(Settings.ModLinksPath));
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

    public byte[] SerializeSetting(Setting setting) => MemoryPackSerializer.Serialize(setting);
    public async ValueTask SerializeSettingAsync(Stream stream, Setting setting) => await MemoryPackSerializer.SerializeAsync(stream, setting);
}