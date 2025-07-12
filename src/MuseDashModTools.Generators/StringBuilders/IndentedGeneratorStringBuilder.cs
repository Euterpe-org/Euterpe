namespace MuseDashModTools.Generators.StringBuilders;

public sealed class IndentedGeneratorStringBuilder : IndentedStringBuilder
{
    public IndentedGeneratorStringBuilder()
    {
        _stringBuilder.AppendLine(Header);
    }

    public override string ToString() => _stringBuilder.ToString();
}