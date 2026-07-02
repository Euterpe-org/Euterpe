namespace Euterpe.Models.Mods;

public static class ModFiles
{
    public const string DllExtension = ".dll";
    public const string DisabledExtension = ".disabled";

    public static bool IsModFile(string filePath) =>
        Path.GetExtension(filePath) is DllExtension or DisabledExtension;
}
