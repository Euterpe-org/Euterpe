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

        var localizeExtensionBuilder = CreateLocalizeExtensionBuilder();
        var localizationManagerBuilder = CreateLocalizationManagerBuilder();
        var providerEnumBuilder = CreateLocalizationProviderBuilder();

        foreach (var data in dataCollection)
        {
            if (data is not var (resourceFile, className))
            {
                continue;
            }

            localizationManagerBuilder.AppendLine($"{className}.Culture = Culture;");
            providerEnumBuilder.AppendLine($"{className},");
            localizeExtensionBuilder.AppendLine($"LocalizationProvider.{className} => new {className}Literal.LocalizedString(resourceKey),");

            GenerateDesignerFile(spc, resourceFile, className);
            GenerateLiteralFile(spc, resourceFile, className);
        }

        localizeExtensionBuilder.ResetIndent();
        localizeExtensionBuilder.AppendLine("""
                                                        _ => throw new UnreachableException()
                                                    };
                                                }
                                            }
                                            """);
        spc.AddSource("LocalizeExtension.cs", localizeExtensionBuilder.ToString());

        localizationManagerBuilder.ResetIndent();
        localizationManagerBuilder.AppendLine("""
                                                      }
                                                  }
                                              }
                                              """);
        spc.AddSource("LocalizationManager.cs", localizationManagerBuilder.ToString());

        providerEnumBuilder.ResetIndent();
        providerEnumBuilder.AppendLine("}");
        spc.AddSource("LocalizationProvider.cs", providerEnumBuilder.ToString());

        GenerateInterface(spc);
        GenerateAssemblyInfo(spc);
    }

    private static IndentedGeneratorStringBuilder CreateLocalizeExtensionBuilder()
    {
        var builder = new IndentedGeneratorStringBuilder();
        builder.AppendLine("""
                           using global::System.Diagnostics;
                           using global::Avalonia.Data;
                           using global::Avalonia.Data.Core;
                           using global::Avalonia.Markup.Xaml;
                           using global::Avalonia.Markup.Xaml.MarkupExtensions;
                           using global::Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
                           using global::MuseDashModTools.Localization;

                           namespace MuseDashModTools.Extensions.MarkupExtensions;

                           public sealed class LocalizeExtension : MarkupExtension
                           {
                               private readonly ILocalizedString? _localizedString;

                               public override CompiledBindingExtension ProvideValue(IServiceProvider serviceProvider)
                               {
                                   var builder = new CompiledBindingPathBuilder();
                                   var clrProperty = new ClrPropertyInfo("Value", o => ((ILocalizedString)o).Value, null, typeof(ILocalizedString));
                                   builder.Property(clrProperty, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor);
                                   return new CompiledBindingExtension
                                   {
                                       Mode = BindingMode.OneWay,
                                       Source = _localizedString,
                                       Path = builder.Build()
                                   };
                               }

                               public LocalizeExtension(string resourceKey) => _localizedString = new XAMLLiteral.LocalizedString(resourceKey);

                               public LocalizeExtension(string resourceKey, LocalizationProvider provider)
                               {
                                   _localizedString = provider switch
                                   {
                           """);
        builder.IncreaseIndent(3);
        return builder;
    }

    private static IndentedGeneratorStringBuilder CreateLocalizationManagerBuilder()
    {
        var builder = new IndentedGeneratorStringBuilder();
        builder.AppendLine($$"""
                             using global::System.Globalization;

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
                                         CultureInfo.CurrentUICulture = value;
                             """);
        builder.IncreaseIndent(3);
        return builder;
    }

    private static IndentedGeneratorStringBuilder CreateLocalizationProviderBuilder()
    {
        var builder = new IndentedGeneratorStringBuilder();
        builder.AppendLine($$"""
                             namespace {{MuseDashModToolsLocalizationNamespace}};

                             public enum LocalizationProvider
                             {
                             """);
        builder.IncreaseIndent();
        return builder;
    }

    private static void GenerateDesignerFile(SourceProductionContext spc, AdditionalText resourceFile, string className)
    {
        var designerBuilder = new GeneratorStringBuilder();
        designerBuilder.AppendLine($$"""
                                     namespace {{MuseDashModToolsLocalizationNamespace}};

                                     public static class {{className}}
                                     {
                                         [field: global::System.Diagnostics.CodeAnalysis.AllowNullAttribute()] [field: global::System.Diagnostics.CodeAnalysis.MaybeNullAttribute()]
                                         public static global::System.Resources.ResourceManager ResourceManager =>
                                             field ??= new global::System.Resources.ResourceManager("{{MuseDashModToolsLocalizationNamespace}}.{{className}}.{{className}}", typeof({{className}}).Assembly);

                                         public static global::System.Globalization.CultureInfo? Culture { get; set; }

                                         public static string GetResourceString(string resourceKey) =>
                                             ResourceManager.GetString(resourceKey, Culture) ?? $"#{resourceKey}#";

                                     """);

        foreach (var (name, value) in ExtractResourceData(resourceFile))
        {
            designerBuilder.AppendLine($"""
                                            /// <summary>
                                            ///     {value.EscapeXmlDoc()}
                                            /// </summary>
                                            public static string {name.GetValidIdentifier()} => GetResourceString("{name}");
                                        """);
        }

        designerBuilder.AppendLine("}");
        spc.AddSource($"{className}.Designer.cs", designerBuilder.ToString());
    }

    private static void GenerateLiteralFile(SourceProductionContext spc, AdditionalText resourceFile, string className)
    {
        var literalBuilder = new GeneratorStringBuilder();
        literalBuilder.AppendLine($$"""
                                    using global::System.ComponentModel;
                                    using global::System.Runtime.CompilerServices;
                                    using global::R3;

                                    namespace {{MuseDashModToolsLocalizationNamespace}};

                                    public static class {{className}}Literal
                                    {
                                    """);

        foreach (var (name, _) in ExtractResourceData(resourceFile))
        {
            literalBuilder.AppendLine($"""
                                           public const string {name.GetValidIdentifier()} = "{name}";

                                       """);
        }

        literalBuilder.AppendLine($$"""
                                        public sealed class LocalizedString : INotifyPropertyChanged, ILocalizedString
                                        {
                                            public string Value => {{className}}.GetResourceString(field);

                                            public LocalizedString(string resourceKey)
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
                                    }
                                    """);

        spc.AddSource($"{className}.Literal.cs", literalBuilder.ToString());
    }

    private static void GenerateInterface(SourceProductionContext spc)
    {
        spc.AddSource("ILocalizedString.cs",
            """
            public interface ILocalizedString
            {
                string Value { get; }
            }
            """);
    }

    private static void GenerateAssemblyInfo(SourceProductionContext spc)
    {
        spc.AddSource("AssemblyInfo.cs",
            """
            using global::Avalonia.Metadata;

            [assembly: XmlnsPrefix("https://github.com/MDModsDev/MuseDashModTools/Localization", "loc")]
            [assembly: XmlnsDefinition("https://github.com/MDModsDev/MuseDashModTools/Localization", "MuseDashModTools.Localization")]
            [assembly: XmlnsDefinition("https://github.com/avaloniaui", "MuseDashModTools.Extensions.MarkupExtensions")]
            """);
    }

    private static IEnumerable<(string Name, string Value)> ExtractResourceData(AdditionalText resourceFile)
    {
        var text = resourceFile.GetText()?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var xdoc = XDocument.Parse(text);
        var dataElements = xdoc.Descendants("data");

        foreach (var element in dataElements)
        {
            var nameAttribute = element.Attribute("name");
            var valueElement = element.Element("value");

            if (nameAttribute != null && valueElement != null)
            {
                yield return (nameAttribute.Value, valueElement.Value.Trim());
            }
        }
    }

    private sealed record ResourceInfo(AdditionalText ResourceFile, string ClassName);
}