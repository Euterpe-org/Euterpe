namespace MuseDashModTools.Abstractions;

public interface IVdfSerializationService
{
    T DeserializeFromStream<T>(Stream stream);
    T DeserializeFromFile<T>(string filePath);
    void SerializeToStream<T>(Stream stream, T data, string name);
    void SerializeToFile<T>(string filePath, T data, string name);
}