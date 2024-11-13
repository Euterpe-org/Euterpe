using System.Runtime.CompilerServices;

namespace MuseDashModTools.Tests;

public static class CurrentFile
{
    public static string FilePath([CallerFilePath] string file = "") => file;

    public static string DirectoryPath([CallerFilePath] string file = "") =>
        Path.GetDirectoryName(file)!;

    public static string RelativePath(string relative, [CallerFilePath] string file = "")
    {
        var directory = Path.GetDirectoryName(file)!;
        return Path.Combine(directory, relative);
    }
}