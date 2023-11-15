using System.IO;

namespace MuseDashModToolsUI.Extensions;

public static class StreamReaderExtensions
{
    /// <summary>
    ///     Read lines using <see cref="StreamReader" /> asynchronously, with start line and end line.
    /// </summary>
    /// <param name="streamReader"></param>
    /// <param name="startLine"></param>
    /// <param name="endLine"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async static Task<string?[]> ReadLineAsync(this StreamReader streamReader, int startLine, int endLine)
    {
        if (startLine > endLine || startLine < 0)
        {
            throw new ArgumentException("startLine must be less than or equal to endLine and non-negative");
        }

        var lines = new List<string?>();

        for (var i = 0; i < startLine; i++)
        {
            await streamReader.ReadLineAsync();
        }

        for (var i = startLine; i <= endLine; i++)
        {
            var line = await streamReader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            lines.Add(line);
        }

        return lines.ToArray();
    }
}