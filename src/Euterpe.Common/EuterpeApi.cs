namespace Euterpe.Common;

public static class EuterpeApi
{
    public const string HttpClientName = "EuterpeApi";
    public const string BaseUrl = "https://euterpe-org.com/api/";

    public static class Telemetry
    {
        public const string VisitorPath = "open/v1/record-visitor";
        public const string DownloadPath = "open/v1/record-download";
    }
}