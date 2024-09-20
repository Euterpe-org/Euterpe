namespace MuseDashModTools.Contracts;

public interface ISerializationService
{
    T? Deserialize<T>(string text);

    string Serialize<T>(T value);
}