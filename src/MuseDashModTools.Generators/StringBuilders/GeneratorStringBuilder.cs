namespace MuseDashModTools.Generators.StringBuilders;

public sealed class GeneratorStringBuilder
{
    private readonly StringBuilder _stringBuilder = new();

    public GeneratorStringBuilder()
    {
        _stringBuilder.AppendLine(Header);
    }

    public void AppendLine() => _stringBuilder.AppendLine();
    public void AppendLine(string value) => _stringBuilder.AppendLine(value);
    public override string ToString() => _stringBuilder.ToString();
}