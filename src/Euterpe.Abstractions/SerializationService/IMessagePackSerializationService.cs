namespace Euterpe.Abstractions;

public interface IMessagePackSerializationService
{
    Manifest DeserializeManifest(Stream stream);
    ValueTask<Manifest> DeserializeManifestAsync(Stream stream, CancellationToken cancellationToken = default);
    ValueTask<Manifest> DeserializeManifestFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    void SerializeManifest(Stream stream, Manifest value);
    Task SerializeManifestAsync(Stream stream, Manifest value, CancellationToken cancellationToken = default);
}