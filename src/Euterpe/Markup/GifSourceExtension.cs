using Avalonia.Labs.Gif;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Markup;

public sealed class GifSourceExtension : MarkupExtension
{
    public string? Uri { get; set; }

    public GifSourceExtension(string uri) => Uri = uri;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Uri))
        {
            throw new InvalidOperationException($"{nameof(GifSourceExtension)}.{nameof(Uri)} must be set.");
        }

        var parsedUri = new Uri(Uri, UriKind.RelativeOrAbsolute);
        if (parsedUri.IsAbsoluteUri)
        {
            return GifStreamSource.FromUri(parsedUri);
        }

        var baseUri = serviceProvider.GetRequiredService<IUriContext>().BaseUri;
        return GifStreamSource.FromUri(parsedUri, baseUri);
    }
}