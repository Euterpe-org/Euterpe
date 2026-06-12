namespace Euterpe.Core;

internal static class LinuxPaths
{
    internal static readonly string[] SteamSearch = new[]
        {
            ".local/share/Steam",
            ".steam/steam",
            ".var/app/ocm.valvesoftware.Steam/data/Steam",
            ".steam/steam",
            ".steam/root"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToArray();
}
