using Euterpe.Shared;
using Euterpe.Shared.Http;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Euterpe.Releaser;

internal static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void RegisterReleaserServices()
        {
            services.AddTransient<XRequestIdHandler>();
            services.AddHttpClient<VelopackApiClient>(static client =>
                {
                    client.BaseAddress = new Uri(EuterpeApi.BaseUrl);
                    client.Timeout = TimeSpan.FromMinutes(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "ApiKey",
                        Environment.GetEnvironmentVariable("EUTERPE_API_KEY"));
                })
                .AddHttpMessageHandler<XRequestIdHandler>()
                .AddStandardResilienceHandler(ConfigureVelopackApiResilience);

            services.AddSingleton<ReleaseProcessRunner>();
            services.AddSingleton<RidReleaseStager>();
        }
    }

    private static void ConfigureVelopackApiResilience(HttpStandardResilienceOptions options)
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(10);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(30);
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.ShouldHandle = static args =>
        {
            var method = args.Context.GetRequestMessage()?.Method;
            return ValueTask.FromResult(
                (method == HttpMethod.Get || method == HttpMethod.Put) &&
                HttpClientResiliencePredicates.IsTransient(args.Outcome));
        };
    }
}
