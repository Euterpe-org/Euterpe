namespace Euterpe.Shared;

public static class EuterpeCdn
{
    public static class Assets
    {
        public const string BaseUrl = "https://assets.euterpe-org.com/";

        public const string ModsJsonUrl = $"{BaseUrl}Mods.json";
        public const string LibsJsonUrl = $"{BaseUrl}Libs.json";
        public const string ModsBaseUrl = $"{BaseUrl}Mods/";
        public const string LibsBaseUrl = $"{BaseUrl}Libs/";
    }

    public static class Releases
    {
        public const string BaseUrl = "https://releases.euterpe-org.com/";
        public const string TagsRssUrl = BaseUrl + "releases.atom";
    }

    public static class Dependencies
    {
        public const string BaseUrl = "https://dependencies.euterpe-org.com/";
    }
}