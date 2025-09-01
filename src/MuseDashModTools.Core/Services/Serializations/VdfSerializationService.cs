using ValveKeyValue;

namespace MuseDashModTools.Core;

internal sealed class VdfSerializationService : IVdfSerializationService
{
    public T DeserializeFromStream<[DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors |
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties)]
        T>(Stream stream)
    {
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        return kv.Deserialize<T>(stream);
    }

    public T DeserializeFromFile<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors |
            DynamicallyAccessedMemberTypes.NonPublicConstructors |
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties)]
        T>(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        return kv.Deserialize<T>(stream);
    }

    public void SerializeToStream<[DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties)]
        T>(Stream stream, T data, string name)
    {
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        kv.Serialize(stream, data, name);
    }

    public void SerializeToFile<[DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicFields |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.NonPublicProperties)]
        T>(string filePath, T data, string name)
    {
        using var stream = File.OpenWrite(filePath);
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        kv.Serialize(stream, data, name);
    }
}