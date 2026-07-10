namespace Euterpe.Core;

internal sealed partial class GameShareService
{
    private const int MaximumShareCodeLength = 16 * 1024;
    private const string ShareLinkPrefix = ISystemAssociationSetup.DeepLinkScheme + "://share/";

    private string CreateShareLink(GameSharePackage package) =>
        ShareLinkPrefix + MessagePackSerialization.SerializeGameSharePackage(package).ToBase64Url();

    private static string? ExtractShareCode(string text)
    {
        var index = text.IndexOf(ShareLinkPrefix, StringComparison.OrdinalIgnoreCase);
        var span = index >= 0 ? text.AsSpan(index + ShareLinkPrefix.Length) : text.AsSpan().Trim();

        var length = 0;
        while (length < span.Length && length <= MaximumShareCodeLength && IsBase64UrlChar(span[length]))
        {
            length++;
        }

        return length is > 0 and <= MaximumShareCodeLength ? span[..length].ToString() : null;
    }

    private static bool IsValidPackage(GameSharePackage package) =>
        package is
        {
            SchemaVersion: GameSharePackage.CurrentSchemaVersion,
            ChartIds: not null,
            Mods: not null
        }
        && Enum.IsDefined(package.GameId)
        && package.ChartIds.Length <= GameSharePackage.MaximumChartCount
        && package.ChartIds.All(static chartId => chartId > 0)
        && package.ChartIds.Distinct().Count() == package.ChartIds.Length
        && package.Mods.All(static mod => mod is not null && !string.IsNullOrWhiteSpace(mod.Name))
        && package.Mods.Select(static mod => mod.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == package.Mods.Length
        && package.ChartIds.Length + package.Mods.Length > 0;

    private static bool IsBase64UrlChar(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';
}
