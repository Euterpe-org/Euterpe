namespace Euterpe.Shared;

public static class EuterpeApi
{
    public const string BaseUrl = "https://euterpe-org.com/api/";

    public static class Auth
    {
        public const string AppToken = "/auth/app/token";
        public const string Refresh = "/auth/refresh";
        public const string Logout = "/auth/logout";
    }

    public static class Telemetry
    {
        public const string BasePath = "telemetry";

        public const string Session = "/session";
    }
}