using MemoryPack;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Formatters;

public sealed class SemanticVersionFormatter : MemoryPackFormatter<SemanticVersion>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref SemanticVersion? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteString(value.ToString());
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref SemanticVersion? value)
    {
        if (reader.PeekIsNull())
        {
            reader.Advance(1);
            value = null;
            return;
        }

        var wrapped = reader.ReadString()!;
        value = SemanticVersion.Parse(wrapped);
    }
}