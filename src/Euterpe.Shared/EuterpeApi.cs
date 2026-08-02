namespace Euterpe.Shared;

public static partial class EuterpeApi
{
    public const string BaseUrl = "https://euterpe-org.com/api/";

    public static partial class Account
    {
        public const string BasePath = "me";
    }

    public static partial class Auth
    {
        public const string BasePath = "auth";

        public const string AppToken = "/app/token";
        public const string Logout = "/logout";
        public const string Refresh = "/refresh";
    }

    public static partial class Charts
    {
        public const string BasePath = "charts";

        public const string CheckUpdates = "/check-updates";
    }

    public static partial class Distribution
    {
        public const string BasePath = "distribution";

        public const string DependenciesPath = "/deps";
        public const string LibsPath = "/libs";
        public const string VelopackPath = "/velopack";
    }

    public static partial class Mods
    {
        public const string BasePath = "mods";

        public const string Manifest = "/app-manifest";
    }

    public static partial class Public
    {
        public const string BasePath = "public";

        public const string Credits = "/credits";
    }

    public static partial class Telemetry
    {
        public const string BasePath = "telemetry";

        public const string Session = "/app/session";
    }
}
