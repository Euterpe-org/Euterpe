using Avalonia.Controls.Templates;

namespace MuseDashModToolsUI;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModels", "Views");
        name = name!.EndsWith("ViewModel") ? name[..^9] : name;
        var type = Type.GetType(name);

        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}