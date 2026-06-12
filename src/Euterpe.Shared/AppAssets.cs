namespace Euterpe.Shared;

public static class AppAssets
{
    public static Uri Uri(string relativePath) => new($"avares://{AppName}/Assets/{relativePath}");
}
