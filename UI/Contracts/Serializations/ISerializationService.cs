namespace MuseDashModToolsUI.Contracts.Serializations;

public interface ISerializationService
{
    T? Deserialize<T>(string text);

    string Serialize<T>(T value);
}