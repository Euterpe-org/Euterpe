namespace Euterpe.Generators;

public sealed partial class RouteGenerator
{
    private static string GenerateViewModelRoutes(
        string viewModelNamespace,
        string viewModelName,
        ImmutableArray<RouteData> children)
    {
        var sb = new IndentedGeneratorStringBuilder();

        sb.AppendLine($$"""
                        using System.Collections.Frozen;

                        namespace {{viewModelNamespace}};

                        partial class {{viewModelName}}
                        {
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            [global::JetBrains.Annotations.UsedImplicitly]
                            public required global::Euterpe.Services.NavigationService NavigationService { get; init; }

                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            public override global::System.Collections.Generic.IReadOnlyList<global::Euterpe.Controls.Models.NavItem> NavItems { get; } =
                            [
                        """);

        sb.IncreaseIndent(2);
        foreach (var child in children)
        {
            sb.AppendLine(BuildNavItem(child));
        }

        sb.ResetIndent();
        sb.AppendLine("    ];");
        sb.AppendLine();

        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            private static readonly FrozenDictionary<string, global::System.Func<global::Euterpe.Services.NavigationService, global::Avalonia.Controls.Control>> RouteLookup =
                                new global::System.Collections.Generic.Dictionary<string, global::System.Func<global::Euterpe.Services.NavigationService, global::Avalonia.Controls.Control>>
                                {
                        """);

        sb.IncreaseIndent(3);
        foreach (var child in children)
        {
            sb.AppendLine($"""["{child.Path}"] = static ns => ns.NavigateTo<global::{child.Namespace}.{child.ClassName}>(),""");
        }

        sb.ResetIndent();
        sb.AppendLine("        }.ToFrozenDictionary();");

        sb.AppendLine($$"""

                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            protected override global::Avalonia.Controls.Control ResolveRoute(string route)
                                => RouteLookup[route](NavigationService);
                        }
                        """);

        return sb.ToString();
    }

    private static string BuildNavItem(RouteData route)
    {
        var iconInitializer = string.IsNullOrEmpty(route.Icon)
            ? string.Empty
            : $$""" { IconResourceKey = "{{route.Icon}}" }""";

        return $"""new(global::Euterpe.Localization.XAMLLiteral.{route.DisplayName}, "{route.Path}"){iconInitializer},""";
    }
}