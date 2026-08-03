using Euterpe.Core.Http.Handlers;
using Euterpe.Core.JsonContexts;
using Refit;

namespace Euterpe.Core.Extensions;

public static class RefitExtensions
{
    private static readonly RefitSettings RefitSettings = CreateRefitSettings();

    internal static RefitSettings CreateRefitSettings() => new()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(SnakeCaseJsonContext.Default.Options)
    };

    public static IHttpClientBuilder AddEuterpeRefitClient<T>(this IServiceCollection services, string name, string basePath, bool authenticated = false)
        where T : class
    {
        var builder = services
            .AddRefitGeneratedClient<T>(RefitSettings, name)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{EuterpeApi.BaseUrl}{basePath}"))
            .AddHttpMessageHandler<XRequestIdHandler>();

        if (authenticated)
        {
            builder.AddHttpMessageHandler<AuthHeaderHandler>();
        }

        return builder;
    }
}
