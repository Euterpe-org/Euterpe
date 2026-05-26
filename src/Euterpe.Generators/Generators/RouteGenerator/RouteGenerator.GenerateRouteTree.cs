namespace Euterpe.Generators;

public sealed partial class RouteGenerator
{
    private static string GenerateRouteTree(RouteTree tree)
    {
        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine("using static Euterpe.IocContainer;");
        cb.AppendLine();
        cb.AppendLine("namespace Euterpe;");
        cb.AppendLine();

        using (cb.Block("public static class RouteTree"))
        {
            cb.AppendLine("""public static readonly global::Euterpe.Controls.Models.RouteNode Root = new("/", null, [""");
            using (cb.Indent())
            {
                var rootChildren = tree.RootChildren;
                for (var i = 0; i < rootChildren.Length; i++)
                {
                    WriteRouteNode(cb, rootChildren[i], i, "/", tree);
                }
            }

            cb.AppendLine("]);");
        }

        return cb.ToString();
    }

    private static void WriteRouteNode(CodeBuilder cb, RouteData route, int index, string parentPath, RouteTree tree)
    {
        var (ns, name) = tree.GetViewModel(parentPath);
        var select = $$"""() => { var vm = Resolve<global::{{ns}}.{{name}}>(); vm.SelectedItem = vm.NavItems[{{index}}]; }""";

        if (!tree.ChildrenByParent.TryGetValue(route.Path, out var children))
        {
            cb.AppendLine($"""new("{route.Path}", {select}, []),""");
            return;
        }

        cb.AppendLine($"""new("{route.Path}", {select}, [""");
        using (cb.Indent())
        {
            for (var i = 0; i < children.Length; i++)
            {
                WriteRouteNode(cb, children[i], i, route.Path, tree);
            }
        }

        cb.AppendLine("]),");
    }
}