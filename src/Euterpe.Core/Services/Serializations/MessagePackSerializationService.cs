using Nerdbank.MessagePack;

namespace Euterpe.Core;

internal sealed class MessagePackSerializationService : IMessagePackSerializationService
{
    private static readonly MessagePackSerializer Serializer = new();

    public Manifest DeserializeManifest(Stream stream) =>
        Serializer.Deserialize<Manifest>(stream)
        ?? throw new InvalidDataException("The MessagePack payload did not contain a manifest.");

    public async ValueTask<Manifest> DeserializeManifestAsync(Stream stream, CancellationToken cancellationToken = default) =>
        await Serializer.DeserializeAsync<Manifest>(stream, cancellationToken).ConfigureAwait(false)
        ?? throw new InvalidDataException("The MessagePack payload did not contain a manifest.");

    public async ValueTask<Manifest> DeserializeManifestFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var stream = File.OpenRead(filePath);
        await using (stream.ConfigureAwait(false))
        {
            return await DeserializeManifestAsync(stream, cancellationToken).ConfigureAwait(false);
        }
    }

    public byte[] SerializeManifest(Manifest value) =>
        Serializer.Serialize(value);

    public void SerializeManifest(Stream stream, Manifest value) =>
        Serializer.Serialize(stream, value);

    public Task SerializeManifestAsync(Stream stream, Manifest value, CancellationToken cancellationToken = default) =>
        Serializer.SerializeAsync(stream, value, cancellationToken).AsTask();
}
