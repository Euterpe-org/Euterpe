namespace MuseDashModTools.Generators.StringBuilders;

public class IndentedStringBuilder
{
    private const byte IndentSize = 4;
    protected readonly StringBuilder StringBuilder = new();

    private int IndentCount { get; set; }

    public IndentedStringBuilder Append(string value)
    {
        DoIndent();
        StringBuilder.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(FormattableString value)
    {
        DoIndent();
        StringBuilder.Append(value);

        return this;
    }

    public IndentedStringBuilder Append(char value)
    {
        DoIndent();
        StringBuilder.Append(value);

        return this;
    }

    public IndentedStringBuilder Append(IEnumerable<string> value)
    {
        DoIndent();
        foreach (var str in value)
        {
            StringBuilder.Append(str);
        }

        return this;
    }

    public IndentedStringBuilder Append(ReadOnlySpan<char> value)
    {
        DoIndent();
        foreach (var chr in value)
        {
            StringBuilder.Append(chr);
        }

        return this;
    }

    public IndentedStringBuilder AppendLine()
    {
        StringBuilder.AppendLine();

        return this;
    }

    public IndentedStringBuilder AppendLine(string value)
    {
        DoIndent();
        StringBuilder.AppendLine(value);

        return this;
    }

    public IndentedStringBuilder AppendLine(FormattableString value)
    {
        DoIndent();
        StringBuilder.Append(value);
        StringBuilder.AppendLine();

        return this;
    }

    public IndentedStringBuilder IncreaseIndent(int count = 1)
    {
        IndentCount += count;

        return this;
    }

    public IndentedStringBuilder DecreaseIndent(int count = 1)
    {
        if (IndentCount > 0)
        {
            IndentCount -= count;
        }

        return this;
    }

    public void ResetIndent() => IndentCount = 0;

    public override string ToString() => StringBuilder.ToString();

    private void DoIndent()
    {
        if (IndentCount > 0)
        {
            StringBuilder.Append(' ', IndentCount * IndentSize);
        }
    }
}