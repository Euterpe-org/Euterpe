using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.IO;
using System.Linq;
using static MuseDashModToolsUI.Utils;

namespace MuseDashModToolsUI.Views;

public partial class MainWindow
{
    /// <summary>
    /// Adds a mod that's installed
    /// </summary>
    internal void AddMod(WebModInfo webMod, LocalModInfo localMod, int index = -1)
    {
        Grid modGrid = new() { Tag = webMod.Name };
        StackPanel expanderPanel = new() { Width = 675 };
        modGrid.Children.Add(expanderPanel);

        Button expanderButton = new()
        {
            Width = 675,
            Height = 75,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = Color_BBB,
            Content = webMod.Name,
            Margin = new Thickness(0, 15, 0, 0),
            BorderBrush = Brushes.Transparent
        };
        expanderButton.Click += Button_Expander;
        expanderPanel.Children.Add(expanderButton);

        StackPanel expanderContent = new()
        {
            Tag = "ExpanderContent",
            IsVisible = false,
            Margin = new Thickness(50, 0, 0, 0)
        };
        expanderPanel.Children.Add(expanderContent);
        expanderContent.Children.Add(new TextBlock { Text = $"{webMod.Description}\n\nAuthor: {webMod.Author}\nVersion:\n" });

        TextBlock versionText = new() { Text = $"{localMod.Version}" };
        var versionDate = new Version(webMod.Version!) > new Version(localMod.Version!) ? -1 : new Version(webMod.Version!) < new Version(localMod.Version!) ? 1 : 0;
        var ShaMismatch = versionDate == 0 && webMod.SHA256 != localMod.SHA256;

        switch (versionDate)
        {
            case -1:
                versionText.Text += $" => {webMod.Version}";
                versionText.Foreground = Color_Red;
                expanderButton.Foreground = Color_Red;
                break;

            case 1:
                versionText.Text += " (WOW MOD DEV)";
                versionText.Foreground = Color_Purple;
                expanderButton.Foreground = Color_Purple;
                break;

            default:
                {
                    if (ShaMismatch)
                    {
                        versionText.Text += $" (Modified)";
                        versionText.Foreground = Color_Yellow;
                        expanderButton.Foreground = Color_Yellow;
                    }

                    break;
                }
        }

        expanderContent.Children.Add(versionText);
        if (webMod.HomePage!.IsValidUrl())
        {
            Button homepageButton = new()
            {
                Content = "Homepage",
                Margin = new Thickness(0, 5, 0, 5),
                Tag = webMod.HomePage
            };
            homepageButton.Click += RoutedOpenUrl;
            expanderContent.Children.Add(homepageButton);
        }

        if (webMod.DependentMods!.Length != 0)
        {
            StackPanel dependencyBlock = new();
            dependencyBlock.Children.Add(new TextBlock { Text = $"Dependencies:" });
            foreach (var githubPath in webMod.DependentMods)
            {
                var temp = WebModsList.Find(x => x.DownloadLink == githubPath);
                var modName = temp == null ? Path.GetFileName(githubPath) : temp.Name;
                TextBlock modBlock = new() { Text = modName };
                if (LocalModsList.All(x => x.Name != modName))
                    modBlock.Foreground = Color_Red;

                dependencyBlock.Children.Add(modBlock);
            }

            expanderContent.Children.Add(dependencyBlock);
        }

        StackPanel controlsPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 10, 0)
        };
        modGrid.Children.Add(controlsPanel);

        var isEnabledBox = new CheckBox()
        {
            IsChecked = !localMod.Disabled,
            Content = "On",
            Tag = webMod.Name,
            Foreground = "#ddd".ToBrush()
        };
        isEnabledBox.Checked += DisableMod;
        isEnabledBox.Unchecked += DisableMod;
        controlsPanel.Children.Add(isEnabledBox);

        Button uninstallButton = new()
        {
            Content = "Uninstall",
            Width = 75,
            Height = 30,
            Tag = webMod.Name,
            Margin = new Thickness(30, LazyMarginLoL, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Background = Color_BG50
        };
        uninstallButton.Click += UninstallMod;
        controlsPanel.Children.Add(uninstallButton);

        if (versionDate != 0 || ShaMismatch)
        {
            Button updateButton = new()
            {
                Width = 75,
                Height = 30,
                Tag = webMod.Name,
                Margin = new Thickness(30, LazyMarginLoL, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = Color_BG50
            };

            if (versionDate == -1)
            {
                updateButton.Content = "Update";
                updateButton.Foreground = Color_Red;
            }
            else if (ShaMismatch || versionDate == 1)
            {
                updateButton.Content = "Reinstall";
                updateButton.Foreground = ShaMismatch ? Color_Yellow : Color_Purple;
            }

            updateButton.Click += InstallModUpdateCall;
            controlsPanel.Children.Add(updateButton);
        }

        if (index == -1)
        {
            ModItemsContainer.Children.Add(modGrid);
            return;
        }

        ModItemsContainer.Children.Insert(index, modGrid);
        ModItemsContainer.Children.RemoveAt(index + 1);
    }

    /// <summary>
    /// Adds a mod that isn't tracked online
    /// </summary>
    internal void AddMod(LocalModInfo localMod)
    {
        Grid modGrid = new() { Tag = localMod.Name };
        ModItemsContainer.Children.Add(modGrid);
        StackPanel expanderPanel = new() { Width = 675 };
        modGrid.Children.Add(expanderPanel);

        Button expanderButton = new()
        {
            Width = 675,
            Height = 75,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = Color_BBB,
            Content = localMod.Name,
            Margin = new Thickness(0, 15, 0, 0),
            BorderBrush = Brushes.Transparent
        };
        expanderButton.Click += Button_Expander;
        expanderPanel.Children.Add(expanderButton);

        StackPanel expanderContent = new()
        {
            Tag = "ExpanderContent",
            IsVisible = false,
            Margin = new Thickness(50, 0, 0, 0)
        };
        expanderPanel.Children.Add(expanderContent);
        expanderContent.Children.Add(new TextBlock
        {
            Text = $"{(string.IsNullOrEmpty(localMod.Description) ? "" : localMod.Description)}\n\nAuthor: {localMod.Author}\nVersion:\n"
        });
        expanderContent.Children.Add(new TextBlock { Text = $"{localMod.Version}" });
        if (localMod.HomePage!.IsValidUrl())
        {
            Button homepageButton = new()
            {
                Content = "Homepage",
                Margin = new Thickness(0, 5, 0, 5),
                Tag = localMod.HomePage
            };
            homepageButton.Click += RoutedOpenUrl;
            expanderContent.Children.Add(homepageButton);
        }

        StackPanel controlsPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 10, 0)
        };
        modGrid.Children.Add(controlsPanel);

        var isEnabledBox = new CheckBox()
        {
            IsChecked = !localMod.Disabled,
            Content = "On",
            Tag = localMod.Name,
            Foreground = "#ddd".ToBrush()
        };
        isEnabledBox.Checked += DisableMod;
        isEnabledBox.Unchecked += DisableMod;
        controlsPanel.Children.Add(isEnabledBox);

        Button downloadButton = new()
        {
            Content = "Uninstall",
            Width = 75,
            Height = 30,
            Tag = localMod.Name,
            Margin = new Thickness(30, LazyMarginLoL, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Background = Color_BG50
        };
        downloadButton.Click += UninstallMod;
        controlsPanel.Children.Add(downloadButton);
    }

    /// <summary>
    /// Adds a mod that isn't installed
    /// </summary>
    internal void AddMod(WebModInfo webMod, int index = -1)
    {
        Grid modGrid = new() { Tag = webMod.Name };
        StackPanel expanderPanel = new() { Width = 675 };
        modGrid.Children.Add(expanderPanel);

        Button expanderButton = new()
        {
            Width = 675,
            Height = 75,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = Color_BBB,
            Content = webMod.Name,
            Margin = new Thickness(0, 15, 0, 0),
            BorderBrush = Brushes.Transparent
        };
        expanderButton.Click += Button_Expander;
        expanderPanel.Children.Add(expanderButton);

        StackPanel expanderContent = new()
        {
            Tag = "ExpanderContent",
            IsVisible = false,
            Margin = new Thickness(50, 0, 0, 0)
        };
        expanderPanel.Children.Add(expanderContent);
        expanderContent.Children.Add(new TextBlock { Text = $"{webMod.Description}\n\nAuthor: {webMod.Author}\nVersion:\n" });
        expanderContent.Children.Add(new TextBlock { Text = $"{webMod.Version}" });
        if (webMod.HomePage!.IsValidUrl())
        {
            Button homepageButton = new()
            {
                Content = "Homepage",
                Margin = new Thickness(0, 5, 0, 5),
                Tag = webMod.HomePage
            };
            homepageButton.Click += RoutedOpenUrl;
            expanderContent.Children.Add(homepageButton);
        }

        if (webMod.DependentMods!.Length != 0)
        {
            StackPanel dependencyBlock = new();
            dependencyBlock.Children.Add(new TextBlock { Text = $"Dependencies:" });
            foreach (var githubPath in webMod.DependentMods)
            {
                var temp = WebModsList.Find(x => x.DownloadLink == githubPath);
                var modName = temp == null ? Path.GetFileName(githubPath) : temp.Name;
                TextBlock modBlock = new()
                {
                    Text = modName
                };
                if (LocalModsList.All(x => x.Name != modName))
                {
                    modBlock.Foreground = Color_Red;
                }

                dependencyBlock.Children.Add(modBlock);
            }

            expanderContent.Children.Add(dependencyBlock);
        }

        StackPanel controlsPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 10, 0)
        };
        modGrid.Children.Add(controlsPanel);

        Button downloadButton = new()
        {
            Content = "Install",
            Width = 75,
            Height = 30,
            Tag = webMod.Name,
            Margin = new Thickness(300, LazyMarginLoL, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Background = Color_BG50
        };
        downloadButton.Click += InstallModUpdateCall;
        controlsPanel.Children.Add(downloadButton);

        if (index == -1)
        {
            ModItemsContainer.Children.Add(modGrid);
            return;
        }

        ModItemsContainer.Children.Insert(index, modGrid);
        ModItemsContainer.Children.RemoveAt(index + 1);
    }
}