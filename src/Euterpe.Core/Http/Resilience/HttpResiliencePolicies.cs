using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Euterpe.Core.Http.Resilience;

internal static class HttpResiliencePolicies
{
    public static void ConfigureApi(HttpStandardResilienceOptions options)
    {
        ConfigureGetRetry(options);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.Retry.MaxRetryAttempts = 3;
    }

    public static void ConfigureHealthCheck(HttpStandardResilienceOptions options)
    {
        ConfigureGetRetry(options);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(35);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.Retry.MaxRetryAttempts = 2;
    }

    private static void ConfigureGetRetry(HttpStandardResilienceOptions options)
    {
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.ShouldHandle = static args =>
        {
            var request = args.Context.GetRequestMessage();
            return ValueTask.FromResult(
                request?.Method == HttpMethod.Get &&
                HttpClientResiliencePredicates.IsTransient(args.Outcome));
        };
    }
}
