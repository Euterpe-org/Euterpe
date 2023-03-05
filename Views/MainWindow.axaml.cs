using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MelonLoader;
using MuseDashModToolsUI.Models;
using static MuseDashModToolsUI.Utils;

namespace MuseDashModToolsUI.Views;

#pragma warning disable SYSLIB0014

public partial class MainWindow : Window
{
    private const string BaseLink = "MDModsDev/ModLinks/dev/";

    private readonly IBrush Color_BBB = "#bbb".ToBrush();
    private readonly IBrush Color_BG50 = "#505050".ToBrush();
    private readonly IBrush Color_Purple = "#a000e6".ToBrush();
    private readonly IBrush Color_Red = "#c80000".ToBrush();
    private readonly IBrush Color_Yellow = "#e19600".ToBrush();
    private readonly IBrush Color_Cyan = "#88b0bb".ToBrush();

    private readonly int LazyMarginLoL = 35;

    private string? CurrentGameDirectory = "D:\\Steam\\steamapps\\common\\Muse Dash";

    private bool localLoadSuccess;
    private bool WebLoadSuccess;

    public MainWindow()
    {
        InitializeComponent();
        // Instance = this;
        // Input_SearchFilter.AddHandler(TextInputEvent, SearchFilterChanged, RoutingStrategies.Tunnel);
        // InitializeSettings();
        // InitializeWebModsList();
        // InitializeLocalModsList();
        // FinishInitialization();
        // Closing += SaveSettings;
    }

    internal static MainWindow? Instance { get; private set; }
    internal int Selected_ModFilter { get; set; } = 0;

    //private Version GameVersion { get; } = null;

    private List<Mod> WebModsList { get; set; } = new();
    private List<Mod> LocalModsList { get; } = new();

    // for early debugging:
    private bool SuccessPopups => false;

    #region Initialize

    /// <summary>
    /// Read settings
    /// </summary>
    private void InitializeSettings()
    {
        return;
        CurrentGameDirectory = null;
        if (!File.Exists("Settings"))
            return;

        var settings = File.ReadAllLines("Settings");
        if (string.IsNullOrEmpty(settings[0]))
            return;

        if (settings[0].IsValidPath())
            CurrentGameDirectory = settings[0];
    }

    /// <summary>
    /// Downloads online mods info
    /// </summary>
    internal void InitializeWebModsList()
    {
        var webClient = new WebClient { Encoding = Encoding.UTF8 };
        try
        {
            string data;
            try
            {
                data = webClient.DownloadString("https://raw.githubusercontent.com/" + BaseLink + "ModLinks.json");
            }
            catch (WebException)
            {
                data = webClient.DownloadString("https://raw.fastgit.org/" + BaseLink + "ModLinks.json");
            }

            WebModsList = JsonSerializer.Deserialize<List<Mod>>(data)!;
            WebLoadSuccess = true;
        }
        catch (Exception)
        {
            DialogPopup("Failed to download online mods info");
            WebLoadSuccess = false;
        }
        finally
        {
            webClient.Dispose();
        }
    }


    /// <summary>
    /// Save game directory in settings
    /// </summary>
    private void SaveSettings(object? sender, CancelEventArgs args)
    {
        File.WriteAllLines("Settings", new string[] { CurrentGameDirectory! });
    }

    #endregion Initialize

    #region Mod Install, Update, Uninstall, Disable

    /// <summary>
    /// Download file from github
    /// </summary>
    private static void ModDownload(string relativeUrl, string path)
    {
        var webClient = new WebClient { Encoding = Encoding.UTF8 };
        try
        {
            try
            {
                webClient.DownloadFile("https://raw.githubusercontent.com/" + BaseLink + relativeUrl, path);
            }
            catch (WebException)
            {
                webClient.DownloadFile("https://raw.fastgit.org/" + BaseLink + relativeUrl, path);
            }
        }
        finally
        {
            webClient.Dispose();
        }
    }
    

    
    /// <summary>
    /// Disable mods
    /// </summary>
    private void ToggleMod(object? sender, RoutedEventArgs args)
    {
        //var isChecked = ((ToggleButton)sender).IsChecked;
        var modName = (string)((Control)sender!).Tag!;
        var localMod = LocalModsList.Find(x => x.Name == modName);
        if (localMod == null)
        {
            ErrorAndExit($"Something went horribly wrong:\nYou somehow disabled {modName},\nwhich doesn't exist!");
        }

        try
        {
            File.Move(
                Path.Join(CurrentGameDirectory, "Mods", localMod!.FileNameExtended()),
                Path.Join(CurrentGameDirectory, "Mods", localMod.FileNameExtended(true)),
                true);

            // Update disabled
            localMod.IsDisabled ^= true;
            return;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    DialogPopup("Mod disable/enable failed\n(unauthorized)");
                    break;

                case IOException:
                    DialogPopup("Mod disable/enable failed\n(is the game running?)");
                    break;

                default:
                    DialogPopup($"Mod disable/enable failed\n({ex.GetType()})");
                    break;
            }

            ((ToggleButton)sender!).IsChecked ^= true;
        }
    }

    #endregion Mod Install, Update, Uninstall, Disable

    #region DisPlay
    
    

    internal void UpdateFilters()
    {
        SearchFilterChanged(Input_SearchFilter, null);
    }

    private bool DoesModPassFilter(string modName)
    {
        var localMod = LocalModsList.Find(x => x.Name == modName);
        var webMod = WebModsList.Find(x => x.Name == modName);
        modName = modName.ToLower();
        switch (Selected_ModFilter)
        {
            case 1:
                if (localMod == null)
                {
                    return false;
                }
                break;

            case 2:
                if (localMod == null || localMod.IsDisabled)
                {
                    return false;
                }
                break;

            case 3:
                if (localMod == null || webMod == null || new Version(webMod.Version!) <= new Version(localMod.Version!))
                {
                    return false;
                }
                break;

            default:
                break;
        }
        return true;
    }

    private void SearchFilterChanged(object? sender, TextInputEventArgs? args)
    {
        var searchTerm = ((TextBox)sender!).Text;
        if (string.IsNullOrEmpty(searchTerm))
        {
            foreach (Control control in ModItemsContainer.Children)
            {
                control.IsVisible = DoesModPassFilter((string)control.Tag!);
            }

            return;
        }

        searchTerm = Regex.Replace(searchTerm, "[ ]{2,}", " ", RegexOptions.None).ToLower();
        var any = false;
        foreach (Control control in ModItemsContainer.Children)
        {
            var modName = (string)control.Tag!;
            control.IsVisible = false;

            if (!DoesModPassFilter(modName))
            {
                continue;
            }

            if (modName.Contains(searchTerm))
            {
                control.IsVisible = true;
                any = true;
            }
        }

        if (any)
        {
            return;
        }

        var splitTerms = searchTerm.Split(' ');
        foreach (Control control in ModItemsContainer.Children)
        {
            var modName = (string)control.Tag!;
            control.IsVisible = false;
            if (!DoesModPassFilter(modName))
            {
                continue;
            }

            control.IsVisible = true;
            foreach (var term in splitTerms)
            {
                if (term == "")
                {
                    continue;
                }

                if (!modName.Contains(term))
                {
                    control.IsVisible = false;
                }
            }
        }
    }

    #endregion DisPlay

    /// <summary>
    /// Load local mod and return its info
    /// </summary>
    private Mod LoadLocalMod(string file)
    {
        var mod = new Mod
        {
            IsDisabled = file.EndsWith(".disabled"),
        };

        mod.FileName = mod.IsDisabled ? Path.GetFileName(file)[..^9] : Path.GetFileName(file);
        var assembly = Assembly.Load(File.ReadAllBytes(file));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);

        mod.Name = attribute.Name;
        mod.Version = attribute.Version;
        if (mod.Name == null || mod.Version == null)
        {
            return null;
        }
        mod.Author = attribute.Author;
        mod.HomePage = attribute.DownloadLink;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(file);

        return mod;
    }
    


    /// <summary>
    /// For xaml
    /// </summary>
    private void OpenModsFolder(object sender, RoutedEventArgs args)
    {
        OpenPath(Path.Join(CurrentGameDirectory, "Mods"));
    }

    /// <summary>
    /// For xaml
    /// </summary>
    private static void RoutedOpenUrl(object? sender, RoutedEventArgs args)
    {
        OpenUrl((string)((Control)sender!).Tag!);
    }

    /// <summary>
    /// Completely erases the contents of the main window and opens a dialog with the given errorMessage.
    /// Once the dialog is closed, the program will exit.
    /// </summary>
    private void ErrorAndExit(string errorMessage)
    {
        DialogPopup(errorMessage, () => { Environment.Exit(0); }, true);
    }

    /// <summary>
    /// Used for implementing expanders, cause the built-in one is glitchy as all hell.
    /// </summary>
    public void Button_Expander(object? sender, RoutedEventArgs args)
    {
        ((Panel)((Control)sender!).Parent!).Children.First(x => (string)((Control)x).Tag! == "ExpanderContent").IsVisible ^= true;
        var parent = ((Control) sender).Parent.Parent.Parent;
        ((ListBoxItem) parent).IsSelected = true;
    }
}