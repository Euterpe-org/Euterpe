namespace MuseDashModToolsUI.Services.Downloads;

public abstract class GitHubServiceBase
{
    protected const string ReleaseApiUrl = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";
    protected const string LatestReleaseApiUrl = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";
    protected const string ModLinksBaseUrl = "MDModsDev/ModLinks/main/";
    protected const string MelonLoaderBaseUrl = "LavaGang/MelonLoader/releases/download/v0.6.1/MelonLoader.x64.zip";
    protected const string UnityDependencyBaseUrl = "LavaGang/Unity-Runtime-Libraries/master/2019.4.32.zip";

    protected const string Cpp2ILBaseUrl =
        "SamboyCoding/Cpp2IL/releases/download/2022.1.0-pre-release.10/Cpp2IL-2022.1.0-pre-release.10-Windows-Netframework472.zip";
}