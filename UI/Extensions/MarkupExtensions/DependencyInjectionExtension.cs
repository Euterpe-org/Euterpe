using Avalonia.Markup.Xaml;

namespace MuseDashModToolsUI.Extensions.MarkupExtensions;

public sealed class DependencyInjectionExtension : MarkupExtension
{
    public static Func<Type?, object>? Resolver { get; set; }
    public Type? Type { get; set; }
    public override object ProvideValue(IServiceProvider serviceProvider) => Resolver?.Invoke(Type)!;
}