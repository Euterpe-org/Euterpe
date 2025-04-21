namespace MuseDashModTools.Common;

public static class GitHubConstants
{
    // GitHub Urls
    public const string GitHubBaseUrl = "https://github.com/";
    public const string GitHubAPIBaseUrl = "https://api.github.com/repos/";
    public const string GitHubRawContentBaseUrl = "https://raw.githubusercontent.com/";

    // Mod Tools Urls
    public const string ModToolsRepoIdentifier = "MDModsDev/MuseDashModTools/";
    public const string ModToolsReleaseDownloadBaseUrl = GitHubBaseUrl + ModToolsRepoIdentifier + "releases/download/";

    // Mod Links Urls
    public const string ModLinksRepoIdentifier = "MDModsDev/ModLinks/";
    public const string ModLinksBaseUrl = ModLinksRepoIdentifier + ModLinksBranch;

    // Download File Urls
    public const string MelonLoaderBaseUrl = $"LavaGang/MelonLoader/releases/download/v{MelonLoaderVersion}/MelonLoader.x64.zip";
    public const string UnityDependencyBaseUrl = $"LavaGang/Unity-Runtime-Libraries/master/{UnityDependencyVersion}.zip";
    public const string Cpp2ILBaseUrl = $"SamboyCoding/Cpp2IL/releases/download/{Cpp2ILVersion}/Cpp2IL-{Cpp2ILVersion}-Windows-Netframework472.zip";
}