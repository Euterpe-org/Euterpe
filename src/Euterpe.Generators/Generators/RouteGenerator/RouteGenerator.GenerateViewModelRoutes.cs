namespace Euterpe.Generators;

public sealed partial class RouteGenerator
{
    private static string GenerateViewModelRoutes(
        string viewModelNamespace,
        string viewModelName,
        ImmutableArray<RouteData> children,
        bool isRoot) =>
        isRoot
            ? GenerateRootRoutes(viewModelNamespace, viewModelName, children)
            : GenerateSwitchingRoutes(viewModelNamespace, viewModelName, children);

    private static string GenerateRootRoutes(
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
                        """);

        foreach (var child in children)
        {
            var vm = $"global::{child.Namespace}.{child.ClassName}";
            var init = child.IsPerGame
                ? $"global::Euterpe.Mvvm.PageHost.PerGame<{vm}>()"
                : $"global::Euterpe.Mvvm.PageHost.App(global::Euterpe.IocContainer.Resolve<{vm}>())";
            sb.AppendLine($"    public global::Euterpe.Mvvm.PageHost {HostName(child)} {{ get; }} = {init};");
        }

        sb.AppendLine();
        sb.AppendLine($$"""
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

        var hostList = string.Join(", ", children.Select(HostName));
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            public override global::System.Collections.Generic.IReadOnlyList<global::Euterpe.Mvvm.PageHost> Pages => field ??= [{{hostList}}];
                        """);

        sb.AppendLine();
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            private static readonly FrozenDictionary<string, int> RouteIndex =
                                new global::System.Collections.Generic.Dictionary<string, int>
                                {
                        """);

        sb.IncreaseIndent(3);
        for (var i = 0; i < children.Length; i++)
        {
            sb.AppendLine($"""["{children[i].Path}"] = {i},""");
        }

        sb.ResetIndent();
        sb.AppendLine("        }.ToFrozenDictionary();");

        sb.AppendLine($$"""

                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            protected override global::Euterpe.Mvvm.PageHost ResolveRoute(string route) => Pages[RouteIndex[route]];
                        }
                        """);

        return sb.ToString();
    }

    private static string GenerateSwitchingRoutes(
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
                            private static readonly FrozenDictionary<string, global::System.Func<global::Autofac.IComponentContext, global::Euterpe.Mvvm.ViewModelBase>> RouteLookup =
                                new global::System.Collections.Generic.Dictionary<string, global::System.Func<global::Autofac.IComponentContext, global::Euterpe.Mvvm.ViewModelBase>>
                                {
                        """);

        sb.IncreaseIndent(3);
        foreach (var child in children)
        {
            sb.AppendLine($"""["{child.Path}"] = static ctx => ctx.Resolve<global::{child.Namespace}.{child.ClassName}>(),""");
        }

        sb.ResetIndent();
        sb.AppendLine("        }.ToFrozenDictionary();");

        sb.AppendLine($$"""

                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            protected override global::Euterpe.Mvvm.ViewModelBase ResolveRoute(string route) => RouteLookup[route](Container);
                        }
                        """);

        return sb.ToString();
    }

    private static string HostName(RouteData route)
    {
        const string suffix = "ViewModel";
        var name = route.ClassName.EndsWith(suffix, StringComparison.Ordinal)
            ? route.ClassName[..^suffix.Length]
            : route.ClassName;
        return name + "Host";
    }

    private static string BuildNavItem(RouteData route)
    {
        var iconInitializer = string.IsNullOrEmpty(route.Icon)
            ? string.Empty
            : $$""" { IconResourceKey = "{{route.Icon}}" }""";

        return $"""new(global::Euterpe.Localization.XAMLLiteral.{route.DisplayName}, "{route.Path}"){iconInitializer},""";
    }
}