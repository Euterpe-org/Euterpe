using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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

namespace MuseDashModToolsUI.Views;

#pragma warning disable SYSLIB0014
#pragma warning disable CS8618
#pragma warning disable CS8600
#pragma warning disable CS8625
#pragma warning disable CS8604
#pragma warning disable CS8601
#pragma warning disable CS8603

public partial class MainWindow : Window
{
    private const string BaseLink = "MDModsDev/ModLinks/dev/";

    private readonly IBrush Color_BBB = "#bbb".ToBrush();
    private readonly IBrush Color_BG50 = "#505050".ToBrush();
    private readonly IBrush Color_Purple = "#a000e6".ToBrush();
    private readonly IBrush Color_Red = "#c80000".ToBrush();
    private readonly IBrush Color_Yellow = "#e19600".ToBrush();

    private readonly int LazyMarginLoL = 35;

    private string? CurrentGameDirectory = Directory.GetCurrentDirectory();

    public bool localLoadSuccess = true;
    private bool WebLoadSuccess;

    public MainWindow()
    {
        InitializeComponent();
        _Instance = this;
        Input_SearchFilter.AddHandler(TextInputEvent, SearchFilterChanged, RoutingStrategies.Tunnel);
        InitializeSettings();
        InitializeWebModsList();
        InitializeLocalModsList();
        FinishInitialization();
        Closing += SaveSettings;
    }

    public static MainWindow _Instance { get; private protected set; } = null;
    public int Selected_ModFilter { get; set; } = 0;
    public Version GameVersion { get; } = null;

    public List<WebModInfo> WebModsList { get; private set; }
    public List<LocalModInfo> LocalModsList { get; } = new();

    // for early debugging:
    public bool SuccessPopups { get; set; } = false;

    public bool SkipCheckbox { get; set; } = false;

    public void ExitProgram()
    {
        Environment.Exit(0);
    }

    /*Completely erases all contents of the main window and opens a dialog with the given errorMessage
     *Once the dialog is exited, the program will close.*/
    public void ErrorAndExit(string errorMessage)
    {
        DialogPopup(errorMessage, ExitProgram, true);
    }

    /* The following functions are shorthands for opening a new DialogWindow,
     * and then returning the created window */
    public DialogWindow DialogPopup(string message, Action exitAction, bool isModal = true)
    {
        DialogWindow dialog = new()
        {
            TextDisplay = message,
            ButtonClickFunction = exitAction
        };
        if (isModal)
        {
            if (!IsVisible)
            {
                Show();
            }

            dialog.ShowDialog(this);
        }
        else
        {
            dialog.Show();
        }

        return dialog;
    }

    public DialogWindow DialogPopup(string message, bool isModal = true) => DialogPopup(message, null, isModal);

    /// <summary>
    /// Downloads online mods
    /// </summary>
    public void InitializeWebModsList()
    {
        try
        {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            string data;
            try
            {
                data = webClient.DownloadString("https://raw.githubusercontent.com/" + BaseLink + "ModLinks.json");
            }
            catch (WebException)
            {
                data = webClient.DownloadString("https://raw.fastgit.org/" + BaseLink + "ModLinks.json");
            }

            webClient.Dispose();
            WebModsList = JsonSerializer.Deserialize<List<WebModInfo>>(data);
            WebLoadSuccess = true;
        }
        catch (Exception)
        {
            DialogPopup("Failed to download online mod info");
            WebLoadSuccess = false;
        }
    }

    /// <summary>
    /// Download file from github
    /// </summary>
    public byte[] ModDownload(string relativeUrl)
    {
        var webClient = new WebClient { Encoding = Encoding.UTF8 };
        try
        {
            return webClient.DownloadData("https://raw.githubusercontent.com/" + BaseLink + relativeUrl);
        }
        catch (WebException)
        {
            try
            {
                return webClient.DownloadData("https://raw.fastgit.org/" + BaseLink + relativeUrl);
            }
            catch (Exception)
            {
                return null;
            }
        }
        finally
        {
            webClient.Dispose();
        }
    }

    //Main function is to compare mods and call each AddMod function appropriately
    internal void FinishInitialization()
    {
        if (!localLoadSuccess) return;

        var isTracked = new bool[LocalModsList.Count];
        foreach (var webMod in WebModsList)
        {
            var mod = webMod;
            var localModIdx = LocalModsList.FindIndex(localMod => localMod.Name == mod.Name);
            if (localModIdx == -1)
            {
                AddMod(webMod);
                continue;
            }

            isTracked[localModIdx] = true;
            AddMod(webMod, LocalModsList[localModIdx]);
        }

        for (var i = 0; i < isTracked.Length; i++)
        {
            if (!isTracked[i])
            {
                AddMod(LocalModsList[i]);
            }
        }
    }

    public void SaveSettings(object? sender, CancelEventArgs args)
    {
        File.WriteAllLines("settings", new string[] { CurrentGameDirectory });
    }

    public void ChoosePath_Call(object? sender, RoutedEventArgs args)
    {
        ChoosePath();
    }

    public async void ChoosePath()
    {
        OpenFolderDialog dialog = new()
        {
            Title = "Choose game path"
        };
        Action? exitAction = CurrentGameDirectory == null ? ChoosePath : null;
        var result = await dialog.ShowAsync(this);
        if (result == null)
        {
            if (CurrentGameDirectory == null)
            {
                ChoosePath();
            }

            return;
        }

        var MDPath = Path.Join(result, "MuseDash.exe");
        var ModsPath = Path.Join(result, "Mods");
        if (File.Exists(MDPath))
        {
            try
            {
                var version = FileVersionInfo.GetVersionInfo(MDPath).FileVersion;
                if (version == null || !version.StartsWith("2019."))
                {
                    DialogPopup("Failed to verify MuseDash.exe\nMake sure you selected the right folder", exitAction);
                    return;
                }
            }
            catch (Exception)
            {
                DialogPopup("Couldn't find MuseDash.exe\nMake sure you selected the right folder", exitAction);
                return;
            }
        }

        if (!Directory.Exists(ModsPath))
        {
            try
            {
                Directory.CreateDirectory(ModsPath);
            }
            catch (Exception)
            {
                DialogPopup("Couldn't find or create the Mods folder", exitAction);
                return;
            }
        }

        ModItemsContainer.Children.Clear();
        LocalModsList.Clear();
        CurrentGameDirectory = result;
        InitializeLocalModsList(false);
        if (!localLoadSuccess)
        {
            DialogPopup("Failed to read local mods", ChoosePath);
            CurrentGameDirectory = null;
            return;
        }

        FinishInitialization();
    }

    public void InitializeSettings()
    {
        CurrentGameDirectory = null;
        if (!File.Exists("settings"))
        {
            return;
        }

        var settings = File.ReadAllLines("settings");
        if (settings[0] == "")
        {
            return;
        }

        if (settings[0].IsValidPath())
        {
            CurrentGameDirectory = settings[0];
        }
    }


    public void UpdateModDisplay(WebModInfo webMod, LocalModInfo localMod)
    {
        for (var i = 0; i < ModItemsContainer.Children.Count; i++)
        {
            if ((string)((Control)ModItemsContainer.Children[i]).Tag == webMod.Name)
            {
                AddMod(webMod, localMod, i);
                break;
            }
        }

        AddMod(webMod, localMod);
    }

    public void UpdateModDisplay(LocalModInfo localMod)
    {
        var webModIdx = WebModsList.FindIndex(x => x.Name == localMod.Name);
        if (webModIdx == -1)
        {
            ModItemsContainer.Children.Remove(ModItemsContainer.Children.First(x => (string)((Control)x).Tag == localMod.Name));
            return;
        }

        var webMod = WebModsList[webModIdx];
        for (var i = 0; i < ModItemsContainer.Children.Count; i++)
        {
            if ((string)((Control)ModItemsContainer.Children[i]).Tag == webMod.Name)
            {
                AddMod(webMod, i);
                break;
            }
        }
    }

    /// <summary>
    /// Load local mod and return its info
    /// </summary>
    internal LocalModInfo LoadLocalMod(string file)
    {
        var tempPath = Path.GetFileName(file);
        if (tempPath.EndsWith(".disabled"))
            tempPath = tempPath[..^9];

        var mod = new LocalModInfo
        {
            Disabled = file.EndsWith(".disabled"),
            FileName = tempPath
        };
        var assembly = Assembly.Load(File.ReadAllBytes(file));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);

        mod.Name = attribute.Name;
        mod.Version = attribute.Version;
        mod.Author = attribute.Author;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(file);

        return mod;
    }

    /// <summary>
    /// Loads installed mods from the mods folder
    /// </summary>
    public void InitializeLocalModsList(bool failPopups = true)
    {
        localLoadSuccess = true;
        if (CurrentGameDirectory == null)
        {
            if (failPopups)
            {
                DialogPopup("No path is set\nNavigate to the game folder", ChoosePath);
            }

            localLoadSuccess = false;
        }

        try
        {
            var path = Path.Join(CurrentGameDirectory, "Mods");
            var files = Directory.GetFiles(path, "*.dll")
                .Concat(Directory.GetFiles(path, "*.dll.disabled"))
                .ToList();
            foreach (var file in files)
            {
                try
                {
                    if (file.EndsWith(".disabled") && files.Contains(file[..^9]))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception)
                        {
                        }

                        continue;
                    }

                    var localMod = LoadLocalMod(file);
                    if (localMod != null)
                    {
                        LocalModsList.Add(localMod);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
            if (failPopups)
            {
                DialogPopup("Failed to read local mods\nMake sure you're in the game directory");
            }

            localLoadSuccess = false;
        }
    }

    public void UpdateFilters()
    {
        SearchFilterChanged(Input_SearchFilter, null);
    }

    public void SearchFilterChanged(object? sender, TextInputEventArgs args)
    {
        var searchTerm = ((TextBox)sender).Text;
        if (string.IsNullOrEmpty(searchTerm))
        {
            foreach (Control control in ModItemsContainer.Children)
            {
                control.IsVisible = true;
            }

            return;
        }

        searchTerm = Regex.Replace(searchTerm, "[ ]{2,}", " ", RegexOptions.None).ToLower();
        var any = false;
        foreach (Control control in ModItemsContainer.Children)
        {
            var modName = (string)control.Tag;
            control.IsVisible = false;
            var localMod = LocalModsList.Find(x => x.Name == modName);
            var webMod = WebModsList.Find(x => x.Name == modName);
            modName = modName.ToLower();
            switch (Selected_ModFilter)
            {
                case 0:
                    break;
                case 1:
                    if (localMod == null)
                    {
                        continue;
                    }

                    break;
                case 2:
                    if (localMod == null || localMod.Disabled)
                    {
                        continue;
                    }

                    break;
                case 3:
                    if (localMod == null || webMod == null || new Version(webMod.Version) <= new Version(localMod.Version))
                    {
                        continue;
                    }

                    break;
                default:
                    break;
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

        string[] splitTerms = searchTerm.Split(' ');
        foreach (Control control in ModItemsContainer.Children)
        {
            var modName = (string)control.Tag;
            control.IsVisible = false;
            var localMod = LocalModsList.Find(x => x.Name == modName);
            var webMod = WebModsList.Find(x => x.Name == modName);
            switch (Selected_ModFilter)
            {
                case 0:
                    break;
                case 1:
                    if (localMod == null)
                    {
                        continue;
                    }

                    break;
                case 2:
                    if (localMod == null || localMod.Disabled)
                    {
                        continue;
                    }

                    break;
                case 3:
                    if (localMod == null || webMod == null || new Version(webMod.Version) <= new Version(localMod.Version))
                    {
                        continue;
                    }

                    break;
                default:
                    break;
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

    public void ModCheckboxChanged(object? sender, RoutedEventArgs args)
    {
        if (SkipCheckbox)
        {
            SkipCheckbox = false;
            return;
        }

        var isChecked = ((ToggleButton)sender).IsChecked;
        var localMod = LocalModsList.Find(x => x.Name == (string)((Control)sender).Tag);
        if (localMod == null)
        {
            throw new NullReferenceException();
        }

        try
        {
            File.Move(
                Path.Join(CurrentGameDirectory, "Mods", localMod.FileName + (localMod.Disabled ? ".disabled" : "")),
                Path.Join(CurrentGameDirectory, "Mods", localMod.FileName + (localMod.Disabled ? "" : ".disabled")),
                true);
            localMod.Disabled ^= true;
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

            SkipCheckbox = true;
            ((ToggleButton)sender).IsChecked ^= true;
            return;
        }
    }

    //Used for implementing expanders, cause the built-in one is glitchy as all hell.
    public void Button_Expander(object? sender, RoutedEventArgs args)
    {
        ((Panel)((Control)sender).Parent)
            .Children
            .First(x =>
                (string)((Control)x).Tag == "ExpanderContent"
            )
            .IsVisible ^= true;
    }

    public void RoutedOpenURL(object? sender, RoutedEventArgs args)
    {
        OpenURL((string)((Control)sender).Tag);
    }

    public bool InstallModUpdate(string modName, bool allowSuccessSetting)
    {
        var webMod = WebModsList.Find(x => x.Name == modName);
        if (webMod == null)
        {
            DialogPopup($"Mod download failed\n(key \"{modName}\" was not present)");
            return false;
        }

        var data = ModDownload(webMod.DownloadLink);
        if (data == null)
        {
            DialogPopup("Mod download failed\n(are you online?)");
            return false;
        }

        var localModIdx = LocalModsList.FindIndex(x => x.Name == webMod.Name);
        var path = Path.Join(CurrentGameDirectory, "Mods", localModIdx == -1 ? webMod.DownloadLink[5..] : LocalModsList[localModIdx].FileName + (LocalModsList[localModIdx].Disabled ? ".disabled" : ""));
        var lastIdx = path.LastIndexOf(Path.DirectorySeparatorChar);
        foreach (var dependencyName in webMod.DependentMods)
        {
            var findMod = WebModsList.Find(x => x.DownloadLink == dependencyName);
            if (!InstallModUpdate(findMod?.Name, false))
            {
                return false;
            }
        }

        try
        {
            File.WriteAllBytes(path, data);
            var localMod = LoadLocalMod(path);
            LocalModsList.Add(LoadLocalMod(path));
            UpdateModDisplay(webMod, localMod);
            if (SuccessPopups && allowSuccessSetting)
            {
                DialogPopup("Download successful.");
            }

            return true;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case SecurityException:
                case UnauthorizedAccessException:
                    DialogPopup("Mod install failed\n(unauthorized)");
                    break;
                case IOException:
                    DialogPopup("Mod install failed\n(is the game running?)");
                    break;
                default:
                    DialogPopup($"Mod install failed\n({ex.GetType()})");
                    break;
            }

            return false;
        }
    }

    public void InstallModUpdateCall(object? sender, RoutedEventArgs args)
    {
        InstallModUpdate((string)((Control)sender).Tag, true);
    }

    public void UninstallMod(object? sender, RoutedEventArgs args)
    {
        var localMod = LocalModsList.Find(x => x.Name == (string)((Control)sender).Tag);

        var path = Path.Join(CurrentGameDirectory, "Mods", localMod.FileName + (localMod.Disabled ? ".disabled" : ""));
        if (!File.Exists(path))
        {
            DialogPopup("Mod uninstall failed\n(file not found)");
            return;
        }

        try
        {
            File.Delete(path);
            LocalModsList.Remove(localMod);
            UpdateModDisplay(localMod);
            if (SuccessPopups)
            {
                DialogPopup("Uninstall successful");
            }
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    DialogPopup("Mod uninstall failed\n(unauthorized)");
                    break;
                case IOException:
                    DialogPopup("Mod uninstall failed\n(is the game running?)");
                    break;
                default:
                    DialogPopup($"Mod uninstall failed\n({ex.GetType()})");
                    break;
            }

            return;
        }
    }

    public void OpenURL(string linkToOpen)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? linkToOpen : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {linkToOpen}" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? linkToOpen : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
        catch (Exception)
        {
        }
    }

    public void OpenPath(string pathToOpen)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? pathToOpen : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-R {pathToOpen}" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? pathToOpen : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
        catch (Exception)
        {
        }
    }

    public void OpenMods(object sender, RoutedEventArgs args)
    {
        OpenPath(Path.Join(CurrentGameDirectory, "Mods"));
    }
}