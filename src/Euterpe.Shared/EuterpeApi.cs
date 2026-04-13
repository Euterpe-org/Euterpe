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

    public static class Account
    {
        public const string BasePath = "me";

        public const string VanillaBinding = "/vanilla-binding";
    }

    public static class Telemetry
    {
        public const string BasePath = "telemetry";

        public const string Session = "/session";
    }

    public static class Mods
    {
        public const string BasePath = "mods";

        public const string Manifest = "/app-manifest";
    }

    public static class Distribution
    {
        public const string BasePath = "distribution";
        public const string LibsPath = "/libs";
        public const string DependenciesPath = "/deps";
        public const string ReleasesPath = "/app-releases";
    }

    public static class Charts
    {
        public const string BasePath = "charts";
    }
}