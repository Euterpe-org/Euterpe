namespace Euterpe.Generators;

public sealed partial class RouteGenerator
{
    private static string GenerateRouteTree(RouteTree tree)
    {
        var sb = new IndentedGeneratorStringBuilder();

        sb.AppendLine("""
                      using static Euterpe.IocContainer;

                      namespace Euterpe;

                      public static class RouteTree
                      {
                          public static readonly global::Euterpe.Controls.Models.RouteNode Root = new("/", null, [
                      """);

        sb.IncreaseIndent(2);
        var rootChildren = tree.RootChildren;
        for (var i = 0; i < rootChildren.Length; i++)
        {
            WriteRouteNode(sb, rootChildren[i], i, "/", tree);
        }

        sb.ResetIndent();

        sb.AppendLine("""
                          ]);
                      }
                      """);

        return sb.ToString();
    }

    private static void WriteRouteNode(IndentedGeneratorStringBuilder sb, RouteData route, int index, string parentPath, RouteTree tree)
    {
        var (viewModelNamespace, viewModelName) = tree.GetViewModel(parentPath);

        var selectLambda =
            $$"""() => { var vm = Resolve<global::{{viewModelNamespace}}.{{viewModelName}}>(); vm.SelectedItem = vm.NavItems[{{index}}]; }""";

        if (!tree.ChildrenByParent.TryGetValue(route.Path, out var children))
        {
            sb.AppendLine($"""new("{route.Path}", {selectLambda}, []),""");
            return;
        }

        sb.AppendLine($"""new("{route.Path}", {selectLambda}, [""");
        sb.IncreaseIndent();

        for (var i = 0; i < children.Length; i++)
        {
            WriteRouteNode(sb, children[i], i, route.Path, tree);
        }

        sb.DecreaseIndent();
        sb.AppendLine("]),");
    }
}