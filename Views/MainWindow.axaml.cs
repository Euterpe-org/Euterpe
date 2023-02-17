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

    private readonly int LazyMarginLoL = 35;

    private string? CurrentGameDirectory = Directory.GetCurrentDirectory();

    private bool localLoadSuccess;
    private bool WebLoadSuccess;

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        Input_SearchFilter.AddHandler(TextInputEvent, SearchFilterChanged, RoutingStrategies.Tunnel);
        InitializeSettings();
        InitializeWebModsList();
        InitializeLocalModsList();
        FinishInitialization();
        Closing += SaveSettings;
    }

    internal static MainWindow? Instance { get; private set; }
    internal int Selected_ModFilter { get; set; } = 0;

    //private Version GameVersion { get; } = null;

    private List<WebModInfo> WebModsList { get; set; } = new();
    private List<LocalModInfo> LocalModsList { get; } = new();

    // for early debugging:
    private bool SuccessPopups => false;

    #region Initialize

    /// <summary>
    /// Read settings
    /// </summary>
    private void InitializeSettings()
    {
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

            WebModsList = JsonSerializer.Deserialize<List<WebModInfo>>(data)!;
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
    /// Loads installed mods from the mods folder
    /// </summary>
    public void InitializeLocalModsList(bool failPopups = true)
    {
        if (CurrentGameDirectory is null)
        {
            if (failPopups)
            {
                DialogPopup("No path is set\nNavigate to the Muse Dash folder", ChoosePath);
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
                    // If disabled and not disabled dll both exist
                    if (file.EndsWith(".disabled") && files.Contains(file[..^9]))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception)
                        {
                            DialogPopup($"Deleting \"{file.Replace(path, null)}\" failed");
                        }

                        continue;
                    }

                    var localMod = LoadLocalMod(file);
                    if (localMod is not null)
                    {
                        LocalModsList.Add(localMod);
                    }
                }
                catch (Exception)
                {
                    DialogPopup($"Add \"{file.Replace(path, null)}\" to mod list failed");
                }
            }

            localLoadSuccess = true;
        }
        catch (Exception)
        {
            if (failPopups)
            {
                DialogPopup("Failed to read local mods\nMake sure you've chose the Muse Dash folder");
            }

            localLoadSuccess = false;
        }
    }

    /// <summary>
    /// Main function is to compare mods and call each AddMod function appropriately
    /// </summary>
    internal void FinishInitialization()
    {
        if (!localLoadSuccess || !WebLoadSuccess) return;

        var isTracked = new bool[LocalModsList.Count];
        foreach (var webMod in WebModsList)
        {
            var localModIdx = LocalModsList.FindIndex(localMod => localMod.Name == webMod.Name);
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
    /// Install mods
    /// </summary>
    private bool InstallModUpdate(string modName, bool allowSuccessSetting)
    {
        var webMod = WebModsList.Find(x => x.Name == modName);
        if (webMod is null)
        {
            DialogPopup($"Mod download failed\n(key \"{modName}\" was not present)");
            return false;
        }

        var localModIdx = LocalModsList.FindIndex(x => x.Name == webMod.Name);
        var path = Path.Join(CurrentGameDirectory, "Mods", localModIdx == -1 ? webMod.DownloadLink![5..] : LocalModsList[localModIdx].FileName);
        //var lastIdx = path.LastIndexOf(Path.DirectorySeparatorChar);
        ModDownload(webMod.DownloadLink!, path);
        foreach (var dependencyName in webMod.DependentMods!)
        {
            var dependentMod = WebModsList.Find(x => x.DownloadLink == dependencyName);
            if (!InstallModUpdate(dependentMod?.Name!, false))
            {
                return false;
            }
        }

        // Load downloaded mod
        try
        {
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
                    DialogPopup("Mod download failed\n(are you online?)");
                    break;
            }
        }
        finally
        {
            webClient.Dispose();
        }
    }

    private void InstallModUpdateCall(object? sender, RoutedEventArgs args)
    {
        InstallModUpdate((string)((Control)sender!).Tag!, true);
    }

    /// <summary>
    /// Uninstall mods
    /// </summary>
    private void UninstallMod(object? sender, RoutedEventArgs args)
    {
        var localMod = LocalModsList.Find(x => x.Name == (string)((Control)sender!).Tag!);

        var path = Path.Join(CurrentGameDirectory, "Mods", localMod!.FileName);
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
        }
    }

    /// <summary>
    /// Disable mods
    /// </summary>
    private void DisableMod(object? sender, RoutedEventArgs args)
    {
        //var isChecked = ((ToggleButton)sender).IsChecked;
        var localMod = LocalModsList.Find(x => x.Name == (string)((Control)sender!).Tag!);
        if (localMod == null)
        {
            throw new NullReferenceException();
        }

        try
        {
            File.Move(
                Path.Join(CurrentGameDirectory, "Mods", localMod.FileName),
                Path.Join(CurrentGameDirectory, "Mods", localMod.Disabled ? localMod.FileName![..^9] : localMod.FileName + ".disabled"),
                true);

            // Update disabled and file name
            localMod.Disabled ^= true;
            localMod.FileName = localMod.Disabled ? localMod.FileName + ".disabled" : localMod.FileName![..^9];
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

    /// <summary>
    /// Update mod display after downloading mod
    /// </summary>
    private void UpdateModDisplay(WebModInfo webMod, LocalModInfo localMod)
    {
        for (var i = 0; i < ModItemsContainer.Children.Count; i++)
        {
            if ((string)((Control)ModItemsContainer.Children[i]).Tag! == webMod.Name)
            {
                AddMod(webMod, localMod, i);
                break;
            }
        }

        AddMod(webMod, localMod);
    }

    /// <summary>
    /// Update mod display after uninstalling mod
    /// </summary>
    private void UpdateModDisplay(LocalModInfo localMod)
    {
        var webModIdx = WebModsList.FindIndex(x => x.Name == localMod.Name);
        if (webModIdx == -1)
        {
            ModItemsContainer.Children.Remove(ModItemsContainer.Children.First(x => (string)((Control)x).Tag! == localMod.Name));
            return;
        }

        var webMod = WebModsList[webModIdx];
        for (var i = 0; i < ModItemsContainer.Children.Count; i++)
        {
            if ((string)((Control)ModItemsContainer.Children[i]).Tag! == webMod.Name)
            {
                AddMod(webMod, i);
                break;
            }
        }
    }

    internal void UpdateFilters()
    {
        SearchFilterChanged(Input_SearchFilter, null);
    }

    private void SearchFilterChanged(object? sender, TextInputEventArgs? args)
    {
        var searchTerm = ((TextBox)sender!).Text;
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
            var modName = (string)control.Tag!;
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
                    if (localMod == null || webMod == null || new Version(webMod.Version!) <= new Version(localMod.Version!))
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

        var splitTerms = searchTerm.Split(' ');
        foreach (Control control in ModItemsContainer.Children)
        {
            var modName = (string)control.Tag!;
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
                    if (localMod == null || webMod == null || new Version(webMod.Version!) <= new Version(localMod.Version!))
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

    #endregion DisPlay

    /// <summary>
    /// Load local mod and return its info
    /// </summary>
    private LocalModInfo LoadLocalMod(string file)
    {
        var mod = new LocalModInfo
        {
            Disabled = file.EndsWith(".disabled"),
            FileName = Path.GetFileName(file)
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
    /// Let user choosing game path
    /// </summary>
    private async void ChoosePath()
    {
        OpenFolderDialog dialog = new() { Title = "Choose Muse Dash Folder" };
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

    /// <summary>
    /// For xaml
    /// </summary>
    private void ChoosePath_Call(object? sender, RoutedEventArgs args)
    {
        ChoosePath();
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
    /// Completely erases all contents of the main window and opens a dialog with the given errorMessage, once the dialog is exited, the program will close.
    /// </summary>
    private void ErrorAndExit(string errorMessage)
    {
        DialogPopup(errorMessage, () => { Environment.Exit(0); }, true);
    }
}