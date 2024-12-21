using System.Text;

namespace MuseDashModTools.Generators;

public class IndentedStringBuilder
{
    private const byte IndentSize = 4;
    private readonly StringBuilder _stringBuilder = new();
    private bool _indentPending = true;

    public int IndentCount { get; private set; }

    public IndentedStringBuilder Append(string value)
    {
        DoIndent();
        _stringBuilder.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(FormattableString value)
    {
        DoIndent();
        _stringBuilder.Append(value);

        return this;
    }

    public IndentedStringBuilder Append(char value)
    {
        DoIndent();
        _stringBuilder.Append(value);

        return this;
    }

    public IndentedStringBuilder Append(IEnumerable<string> value)
    {
        DoIndent();
        foreach (var str in value)
        {
            _stringBuilder.Append(str);
        }

        return this;
    }

    public IndentedStringBuilder Append(IEnumerable<char> value)
    {
        DoIndent();
        foreach (var chr in value)
        {
            _stringBuilder.Append(chr);
        }

        return this;
    }

    public IndentedStringBuilder AppendLine()
    {
        AppendLine(string.Empty);

        return this;
    }

    public IndentedStringBuilder AppendLine(string value)
    {
        if (value.Length != 0)
        {
            DoIndent();
        }

        _stringBuilder.AppendLine(value);
        _indentPending = true;

        return this;
    }

    public IndentedStringBuilder AppendLine(FormattableString value)
    {
        DoIndent();
        _stringBuilder.Append(value);
        _indentPending = true;

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

    public override string ToString() => _stringBuilder.ToString();

    private void DoIndent()
    {
        if (_indentPending && IndentCount > 0)
        {
            _stringBuilder.Append(' ', IndentCount * IndentSize);
        }

        _indentPending = false;
    }
}