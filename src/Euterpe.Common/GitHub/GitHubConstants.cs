namespace Euterpe.Common;

public static class GitHubConstants
{
    // GitHub Urls
    public const string GitHubBaseUrl = "https://github.com/";
    public const string GitHubAPIBaseUrl = "https://api.github.com/repos/";
    public const string GitHubRawContentBaseUrl = "https://raw.githubusercontent.com/";

    // Mod Tools Urls
    public const string ModToolsRepoIdentifier = "Euterpe-org/Euterpe/";
    public const string ModToolsReleaseDownloadBaseUrl = GitHubBaseUrl + ModToolsRepoIdentifier + "releases/download/";

    // Mod Links Urls
    public const string ModLinksRepoIdentifier = "Euterpe-org/ModLinks/";
    public const string ModLinksBaseUrl = ModLinksRepoIdentifier + ModLinksBranch;

    // Download File Urls
    public const string MelonLoaderBaseUrl = $"LavaGang/MelonLoader/releases/download/v{MelonLoaderVersion}/MelonLoader.x64.zip";

    public const string UnityDependencyBaseUrl = "LavaGang/MelonLoader.UnityDependencies/releases/download/";

    private const string Cpp2ILBaseUrl = $"SamboyCoding/Cpp2IL/releases/download/{Cpp2ILVersion}/";
    public const string Cpp2ILExecutableBaseUrl = $"{Cpp2ILBaseUrl}Cpp2IL-{Cpp2ILVersion}-Windows.exe";
    public const string Cpp2ILPluginBaseUrl = $"{Cpp2ILBaseUrl}Cpp2IL.Plugin.StrippedCodeRegSupport.dll";
}