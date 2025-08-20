namespace MuseDashModTools.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<DownloadSource>))]
public enum DownloadSource
{
    GitHub,
    GitHubMirror,
    Gitee,
    Website
}