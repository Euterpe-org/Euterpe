using System.Xml.Linq;

namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ResourceKeysGenerator : IncrementalGeneratorBase
{
    protected override string ExpectedRootNamespace => MuseDashModToolsLocalizationNamespace;

    protected override void InitializeCore(
        IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<bool> isValidProvider)
    {
        var resourceFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase));

        var resources = resourceFiles
            .Select((resourceFile, _) =>
            {
                var className = Path.GetFileNameWithoutExtension(resourceFile.Path);
                return new ResourceInfo(resourceFile, className);
            })
            .Collect();

        context.RegisterSourceOutput(resources.WithCondition(isValidProvider), GenerateFromData);
    }

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ResourceInfo> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        foreach (var data in dataCollection)
        {
            if (data is not var (resourceFile, className))
            {
                continue;
            }

            var sb = new IndentedStringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine($$"""
                            namespace {{MuseDashModToolsLocalizationNamespace}}
                            {
                                public static partial class {{className}}
                                {
                            """);

            sb.IncreaseIndent(2);
            sb.AppendLine($$"""
                            public static global::System.Resources.ResourceManager ResourceManager =>
                            field ??= new global::System.Resources.ResourceManager(
                                "{{MuseDashModToolsLocalizationNamespace}}.{{className}}",
                                typeof({{className}}).Assembly
                            );

                            public static global::System.Globalization.CultureInfo? Culture { get; set; }
                            """);

            var xdoc = XDocument.Parse(resourceFile.GetText()!.ToString());
            var dataElements = xdoc.Descendants("data");

            foreach (var element in dataElements)
            {
                var name = element.Attribute("name")!.Value;
                var value = element.Element("value")!.Value.Trim();

                sb.AppendLine($"""
                               /// <summary>
                               /// {value.EscapeXmlDoc()}
                               /// </summary>
                               public static string {name.GetValidIdentifier()} =>
                                   ResourceManager?.GetString("{name}");
                               """);
            }

            sb.ResetIndent();
            sb.AppendLine("""
                              }
                          }
                          """);
            spc.AddSource($"{className}.Designer.g.cs", sb.ToString());
        }
    }

    private sealed record ResourceInfo(AdditionalText ResourceFile, string ClassName);
}