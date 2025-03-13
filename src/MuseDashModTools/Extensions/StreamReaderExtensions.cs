namespace MuseDashModTools.Extensions;

public static class StreamReaderExtensions
{
    /// <summary>
    ///     Read lines using <see cref="StreamReader" /> asynchronously, with start line and end line.
    ///     Line number start with 1
    /// </summary>
    /// <param name="streamReader"></param>
    /// <param name="startLine"></param>
    /// <param name="endLine"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<string?[]> ReadLineAsync(this StreamReader streamReader, int startLine, int endLine)
    {
        if (startLine > endLine || startLine < 1)
        {
            throw new ArgumentException("StartLine must be less than or equal to EndLine and start with 1");
        }

        var lines = new List<string?>();

        for (var i = 0; i < startLine; i++)
        {
            await streamReader.ReadLineAsync().ConfigureAwait(false);
        }

        for (var i = startLine; i <= endLine; i++)
        {
            var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            lines.Add(line);
        }

        return lines.ToArray();
    }

    /// <summary>
    ///     Read lines using <see cref="StreamReader" /> asynchronously, with range
    ///     Line number start with 1
    /// </summary>
    /// <param name="streamReader"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<string?[]> ReadLineAsync(this StreamReader streamReader, Range range)
    {
        var startLine = range.Start.Value;
        var endLine = range.End.Value;
        if (startLine > endLine || startLine < 1)
        {
            throw new ArgumentException("StartLine must be less than or equal to EndLine and start with 1");
        }

        var lines = new List<string?>();

        for (var i = 0; i < startLine; i++)
        {
            await streamReader.ReadLineAsync().ConfigureAwait(false);
        }

        for (var i = startLine; i <= endLine; i++)
        {
            var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            lines.Add(line);
        }

        return lines.ToArray();
    }
}