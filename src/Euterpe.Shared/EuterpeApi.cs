namespace Euterpe.Shared;

public static class EuterpeApi
{
    public const string BaseUrl = "https://euterpe-org.com/api/";

    public static class Auth
    {
        public const string BasePath = "auth";

        public const string AppToken = "/app/token";
        public const string Refresh = "/refresh";
        public const string Logout = "/logout";
    }

    public static class Me
    {
        public const string BasePath = "me";
    }

    public static class Telemetry
    {
        public const string BasePath = "telemetry";

        public const string Session = "/session";
    }

    public static class Mod
    {
        public const string BasePath = "mods";

        public const string Manifest = "/app-manifest";
    }

    public static class Distribution
    {
        public const string BasePath = "distribution";

        public static class Libs
        {
            public const string Path = "/libs";
            public const string Manifest = "/manifest";
        }

        public static class Dependencies
        {
            public const string Path = "/deps";
            public const string Manifest = "/manifest";
        }

        public static class Releases
        {
            public const string Path = "/app-releases";
            public const string Manifest = "/manifest";
        }
    }

    public static class Chart
    {
        public const string BasePath = "charts";
    }
}