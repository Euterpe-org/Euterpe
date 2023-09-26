using System.Text.Json;

namespace MuseDashModToolsUI.Contracts;

public interface ISerializeService
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
    ///     Deserialize Setting from Json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    Setting? DeserializeSetting(string json);

    /// <summary>
    ///     General serialize to Json method
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    string SerializeToJson(object obj, JsonSerializerOptions options);

    /// <summary>
    ///     Serialize Setting to Json
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    string SerializeSetting(Setting setting);
}