using Euterpe.Models.Serialization;
using MessagePack;
using MessagePack.Resolvers;

namespace Euterpe.Core;

internal sealed class MessagePackSerializationService : IMessagePackSerializationService
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(EuterpeResolver.Instance, StandardResolver.Instance))
            .WithSecurity(MessagePackSecurity.UntrustedData);

    public Manifest DeserializeManifest(Stream stream) =>
        MessagePackSerializer.Deserialize<Manifest>(stream, Options);

    public ValueTask<Manifest> DeserializeManifestAsync(Stream stream, CancellationToken cancellationToken = default) =>
        MessagePackSerializer.DeserializeAsync<Manifest>(stream, Options, cancellationToken);

    public void SerializeManifest(Stream stream, Manifest value) =>
        MessagePackSerializer.Serialize(stream, value, Options);

    public Task SerializeManifestAsync(Stream stream, Manifest value, CancellationToken cancellationToken = default) =>
        MessagePackSerializer.SerializeAsync(stream, value, Options, cancellationToken);
}