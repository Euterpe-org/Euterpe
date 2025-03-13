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

        var sb = new IndentedStringBuilder();
        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        namespace {{MuseDashModToolsLocalizationNamespace}};

                        public static class LocalizationManager
                        {
                            public static event EventHandler? CultureChanged;

                            public static global::System.Globalization.CultureInfo? Culture
                            {
                                get => field;
                                set
                                {
                                    field = value;
                                    CultureChanged?.Invoke(null, EventArgs.Empty);
                        """);

        sb.IncreaseIndent(3);
        foreach (var data in dataCollection)
        {
            if (data is not var (resourceFile, className))
            {
                continue;
            }

            sb.AppendLine($"{className}.Culture = Culture;");
            GenerateDesignerFile(spc, resourceFile, className);
        }

        sb.ResetIndent();
        sb.AppendLine("""
                              }
                          }
                      }
                      """);

        spc.AddSource("LocalizationManager.cs", sb.ToString());
    }

    private static void GenerateDesignerFile(SourceProductionContext spc, AdditionalText resourceFile, string className)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        using global::System.ComponentModel;
                        using global::System.Runtime.CompilerServices;
                        using global::R3;

                        namespace {{MuseDashModToolsLocalizationNamespace}};

                        public static partial class {{className}}
                        {
                            [field: global::System.Diagnostics.CodeAnalysis.AllowNullAttribute()] [field: global::System.Diagnostics.CodeAnalysis.MaybeNullAttribute()]
                            public static global::System.Resources.ResourceManager ResourceManager =>
                                field ??= new global::System.Resources.ResourceManager("{{MuseDashModToolsLocalizationNamespace}}.{{className}}.{{className}}", typeof({{className}}).Assembly);

                            public static global::System.Globalization.CultureInfo? Culture { get; set; }

                            public static string GetResourceString(string resourceKey) =>
                                ResourceManager.GetString(resourceKey, Culture) ?? $"#{resourceKey}#";

                        """);

        var xdoc = XDocument.Parse(resourceFile.GetText()!.ToString());
        var dataElements = xdoc.Descendants("data");

        foreach (var element in dataElements)
        {
            var name = element.Attribute("name")!.Value;
            var value = element.Element("value")!.Value.Trim();

            sb.AppendLine($"""
                               /// <summary>
                               ///     {value.EscapeXmlDoc()}
                               /// </summary>
                               public static string {name.GetValidIdentifier()} => GetResourceString("{name}");

                               public const string {name.GetValidIdentifier()}Literal = "{name}";

                           """);
        }

        sb.AppendLine($$"""
                            public sealed class LocalizedString : INotifyPropertyChanged
                            {
                                public string Value => {{className}}.GetResourceString(field);

                                private LocalizedString(string resourceKey)
                                {
                                    Value = resourceKey;
                                    Observable.FromEventHandler(
                                            h => LocalizationManager.CultureChanged += h,
                                            h => LocalizationManager.CultureChanged -= h)
                                        .Subscribe(this, (_, state) => state.OnPropertyChanged(nameof(Value)));
                                }

                                public event PropertyChangedEventHandler? PropertyChanged;

                                public override string ToString() => Value;

                                public static implicit operator LocalizedString(string resourceKey) => new(resourceKey);

                                private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                            }
                        """);
        sb.AppendLine("}");
        spc.AddSource($"{className}.Designer.cs", sb.ToString());
    }

    private sealed record ResourceInfo(AdditionalText ResourceFile, string ClassName);
}