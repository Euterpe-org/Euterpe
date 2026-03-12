namespace Euterpe.Common;

public static class EuterpeApi
{
    public const string BaseUrl = "https://euterpe-org.com";

    public static class Telemetry
    {
        public const string VisitorPath = "/api/open/v1/record-visitor";
        public const string DownloadPath = "/api/open/v1/record-download";
    }
}