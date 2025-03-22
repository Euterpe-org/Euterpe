namespace MuseDashModTools.Tasks;

[PublicAPI]
public sealed class GenerateModToolsStyles : Task
{
    private readonly Dictionary<string, (string Comment, List<string> List)> _categories = new()
    {
        ["ExtendedControls"] = ("Extended Controls", []),
        ["TemplatedControls"] = ("Templated Controls", []),
        ["Styles"] = ("Control Styles", [])
    };

    [Required]
    public ITaskItem[] AvaloniaResourceFiles { get; set; } = null!;

    public override bool Execute()
    {
        try
        {
            var sb = new IndentedStringBuilder();
            sb.AppendLine("""
                          <Styles xmlns="https://github.com/avaloniaui"
                                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                                  xmlns:semi="https://irihi.tech/semi">

                              <semi:SemiTheme />

                              <Styles.Resources>
                                  <ResourceDictionary>
                                      <ResourceDictionary.ThemeDictionaries>
                                          <ResourceInclude x:Key="Light" Source="/Resources/LightResource.axaml" />
                                          <ResourceInclude x:Key="Dark" Source="/Resources/DarkResource.axaml" />
                                      </ResourceDictionary.ThemeDictionaries>
                                      <ResourceDictionary.MergedDictionaries>
                                          <ResourceInclude Source="/Resources/SharedResource.axaml" />
                                      </ResourceDictionary.MergedDictionaries>
                                  </ResourceDictionary>
                              </Styles.Resources>
                          """);

            foreach (var file in AvaloniaResourceFiles)
            {
                var directory = Path.GetDirectoryName(file.ItemSpec);
                var fileName = file.GetMetadata("Filename");

                if (_categories.TryGetValue(directory, out var category))
                {
                    category.List.Add($"""<StyleInclude Source="/{directory}/{fileName}.axaml" />""");
                }
            }

            sb.IncreaseIndent();
            foreach (var (_, (comment, lines)) in _categories)
            {
                if (lines is [])
                {
                    continue;
                }

                sb.AppendLine();
                sb.AppendLine($"<!--  {comment}  -->");
                lines.ForEach(x => sb.AppendLine(x));
            }

            sb.ResetIndent();
            sb.Append("</Styles>");

            File.WriteAllText("ModToolsStyles.axaml", sb.ToString());
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }
}