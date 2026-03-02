namespace Euterpe.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<DownloadSource>))]
public enum DownloadSource
{
    Official,
    GitHub,
    Gitee,
    GitHubMirror
}