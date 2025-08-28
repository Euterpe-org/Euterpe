namespace MuseDashModTools.Generators.StringBuilders;

public sealed class IndentedGeneratorStringBuilder : IndentedStringBuilder
{
    public IndentedGeneratorStringBuilder()
    {
        StringBuilder.AppendLine(Header);
    }

    public override string ToString() => StringBuilder.ToString();
}