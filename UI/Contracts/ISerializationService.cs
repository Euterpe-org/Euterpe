using System.Text.Json;

namespace MuseDashModToolsUI.Contracts;

public interface ISerializationService
{
    /// <summary>
    ///     General deserialize to Json method
    /// </summary>
    /// <param name="json"></param>
    /// <param name="options"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? DeserializeFromJson<T>(string json, JsonSerializerOptions options);

    /// <summary>
    ///     General deserialize to Json method
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? DeserializeFromJson<T>(string json);

    /// <summary>
    ///     Deserialize Setting from Json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    Setting? DeserializeSetting(string json);

    /// <summary>
    ///     Deserialize Mod list from Json
    /// </summary>
    /// <returns></returns>
    Task<List<Mod>?> DeserializeModListAsync();

    /// <summary>
    ///     General serialize to Json method
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    string SerializeToJson(object obj, JsonSerializerOptions options);

    /// <summary>
    ///     General serialize to Json method
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    string SerializeToJson(object obj);

    /// <summary>
    ///     Serialize Setting to Json
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    string SerializeSetting(Setting setting);
}