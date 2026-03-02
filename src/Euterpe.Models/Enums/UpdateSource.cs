namespace Euterpe.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<UpdateSource>))]
public enum UpdateSource
{
    Official,
    GitHubAPI,
    GitHubRSS
}