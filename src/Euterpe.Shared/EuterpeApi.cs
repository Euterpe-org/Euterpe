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
        public const string Me = "/me";
    }

    public static class Telemetry
    {
        public const string BasePath = "telemetry";

        public const string Session = "/session";
    }

    public static class Mod
    {
        public const string BasePath = "mods";
    }

    public static class Chart
    {
        public const string BasePath = "charts";
    }
}