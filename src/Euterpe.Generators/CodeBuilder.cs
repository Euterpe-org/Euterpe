namespace Euterpe.Generators;

internal sealed class CodeBuilder
{
    private const int IndentSize = 4;
    private readonly StringBuilder _sb = new();
    private bool _atLineStart = true;
    private int _indent;

    public CodeBuilder Append(string text)
    {
        WriteIndentIfNeeded();
        _sb.Append(text);
        return this;
    }

    public CodeBuilder AppendLine()
    {
        _sb.AppendLine();
        _atLineStart = true;
        return this;
    }

    public CodeBuilder AppendLine(string text)
    {
        if (text.Length is 0)
        {
            return AppendLine();
        }

        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed.Length > 0)
            {
                WriteIndentIfNeeded();
                _sb.Append(trimmed);
            }

            _sb.AppendLine();
            _atLineStart = true;
        }

        return this;
    }

    /// <summary>Writes <paramref name="header" /> (if any) and an opening brace, then indents until disposed.</summary>
    public Scope Block(string? header = null)
    {
        if (header is not null)
        {
            AppendLine(header);
        }

        AppendLine("{");
        _indent++;
        return new Scope(this, true);
    }

    /// <summary>Indents until disposed, without emitting braces (for collection/dictionary initializers).</summary>
    public Scope Indent()
    {
        _indent++;
        return new Scope(this, false);
    }

    public override string ToString() => _sb.ToString();

    private void WriteIndentIfNeeded()
    {
        if (!_atLineStart)
        {
            return;
        }

        if (_indent > 0)
        {
            _sb.Append(' ', _indent * IndentSize);
        }

        _atLineStart = false;
    }

    internal readonly struct Scope(CodeBuilder owner, bool closeBrace) : IDisposable
    {
        public void Dispose()
        {
            owner._indent--;
            if (closeBrace)
            {
                owner.AppendLine("}");
            }
        }
    }
}
