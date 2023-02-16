using Avalonia.Controls;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text;
using System;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using System.Reflection;
using MelonLoader;
using System.Security.Cryptography;
using System.Data;
using System.Collections;
using MuseDashModToolsUI.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Layout;
using Avalonia.Input;
using Harmony;
using Avalonia.Media;
using System.Security;
using Avalonia.Controls.Primitives;
using DynamicData;
using Avalonia.Styling;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MuseDashModToolsUI.Views
{

#pragma warning disable SYSLIB0014
#pragma warning disable CS8618
#pragma warning disable CS8600
#pragma warning disable CS8625
#pragma warning disable CS8604
#pragma warning disable CS8601
#pragma warning disable CS8603

    public static class Extensions
    {
        public static IBrush ToBrush(this string HexColorString)
        {
            return (IBrush)new BrushConverter().ConvertFromString(HexColorString);
        }
        public static bool IsValidUrl(this string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        public static bool IsValidPath(this string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeFile);
        }
    }
    public partial class MainWindow : Window
    {
        public static MainWindow _Instance { get; private protected set; } = null;
        public int Selected_ModFilter { get; set; } = 0;
        public Version GameVersion { get; private set; } = null;

        public List<WebModInfo> WebModsList { get; private set; }
        public List<LocalModInfo> LocalModsList { get; private set; } = new();

        // for early debugging:
        public bool SuccessPopups { get; set; } = false;

        //public string CurrentGameDirectory = Directory.GetCurrentDirectory();

        public string? CurrentGameDirectory;

        private const string BaseLink = "MDModsDev/ModLinks/dev/";

        private int LazyMarginLoL = 35;

        public static IBrush Color_BG50 = "#505050".ToBrush();
        public static IBrush Color_Red = "#c80000".ToBrush();
        public static IBrush Color_BBB = "#bbb".ToBrush();
        public static IBrush Color_Purple = "#a000e6".ToBrush();
        public static IBrush Color_Yellow = "#e19600".ToBrush();

        public bool localLoadSuccess = true;
        public bool webLoadSuccess = true;

        public bool SkipCheckbox { get; set; } = false;
        public MainWindow()
        {
            InitializeComponent();
            _Instance = this;
            Input_SearchFilter.AddHandler(InputElement.TextInputEvent, SearchFilterChanged, RoutingStrategies.Tunnel);
            InitializeSettings();
            InitializeWebModsList();
            InitializeLocalModsList();
            FinishInitialization();
            this.Closing += SaveSettings;
        }

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
                if (!this.IsVisible)
                {
                    this.Show();
                }
                dialog.ShowDialog(this);
            }
            else
            {
                dialog.Show();
            }
            return dialog;
        }

        public DialogWindow DialogPopup(string message, bool isModal = true)
        {
            return DialogPopup(message, null, isModal);
        }

        //Downloads online mods
        public void InitializeWebModsList()
        {
            try
            {
                var webClient = new WebClient
                {
                    Encoding = Encoding.UTF8
                };
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
            }
            catch (Exception)
            {
                DialogPopup("Failed to download\nonline mod info");
                webLoadSuccess = false;
            }
        }

        public byte[] GithubFileDownload(string relativeURL)
        {
            var webClient = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            byte[] data;
            try
            {
                data = webClient.DownloadData("https://raw.githubusercontent.com/" + BaseLink + relativeURL);
                webClient.Dispose();
                return data;
            }
            catch (WebException)
            {
                try
                {

                    data = webClient.DownloadData("https://raw.fastgit.org/" + BaseLink + relativeURL);
                    webClient.Dispose();
                    return data;

                }
                catch (Exception)
                {
                    webClient.Dispose();
                    return null;
                }
            }
        }

        //Main function is to compare mods and call each AddMod function appropriately
        public void FinishInitialization()
        {
            if (localLoadSuccess == false)
            {
                return;
            }
            bool[] isTracked = new bool[LocalModsList.Count];
            for (int i = 0; i < WebModsList.Count; i++)
            {
                WebModInfo webMod = WebModsList[i];
                int localModIdx = LocalModsList.FindIndex(localMod => localMod.Name == webMod.Name);
                if (localModIdx == -1)
                {
                    AddMod(webMod);
                    continue;
                }
                isTracked[localModIdx] = true;
                AddMod(webMod, LocalModsList[localModIdx]);
            }

            for (int i = 0; i < isTracked.Length; i++)
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
            Action? exitAction = (CurrentGameDirectory == null) ? ChoosePath : null;
            string? result = await dialog.ShowAsync(this);
            if (result == null)
            {
                if (CurrentGameDirectory == null)
                {
                    ChoosePath();
                }
                return;
            }
            string MDPath = Path.Join(result, "MuseDash.exe");
            string ModsPath = Path.Join(result, "Mods");
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
            if (localLoadSuccess == false)
            {
                DialogPopup("Failed to read local mods", ChoosePath);
                CurrentGameDirectory = null;
                return;
            }
            FinishInitialization();
        }

        public void InitializeSettings()
        {
            if (!File.Exists("settings"))
            {
                return;
            }
            string[] settings = File.ReadAllLines("settings");
            if (settings[0] == "")
            {
                return;
            }
            if (settings[0].IsValidPath())
            {
                CurrentGameDirectory = settings[0];
            };
        }

        //Adds a mod that's installed
        public void AddMod(WebModInfo webMod, LocalModInfo localMod, int index = -1)
        {
            Grid modGrid = new()
            {
                Tag = webMod.Name
            };
            StackPanel expanderPanel = new()
            {
                Width = 675
            };
            modGrid.Children.Add(expanderPanel);

            Button expanderButton = new()
            {
                Width = 675,
                Height = 75,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = Color_BBB,
                Content = webMod.Name,
                Margin = new(0, 15, 0, 0),
                BorderBrush = Brushes.Transparent
            };
            expanderButton.Click += Button_Expander;
            expanderPanel.Children.Add(expanderButton);

            StackPanel expanderContent = new()
            {
                Tag = "ExpanderContent",
                IsVisible = false,
                Margin = new(50, 0, 0, 0)
            };
            expanderPanel.Children.Add(expanderContent);
            expanderContent.Children.Add(new TextBlock
            {
                Text = $"{webMod.Description}\n\nAuthor: {webMod.Author}\nVersion:\n",
            });
            TextBlock versionText = new()
            {
                Text = $"{localMod.Version}",
            };
            int versionDate = new Version(webMod.Version) > new Version(localMod.Version) ? -1 : (new Version(webMod.Version) < new Version(localMod.Version) ? 1 : 0);
            bool ShaMismatch = versionDate == 0 & webMod.SHA256 != localMod.SHA256;
            if (versionDate == -1)
            {
                versionText.Text += $" => {webMod.Version}";
                versionText.Foreground = Color_Red;
                expanderButton.Foreground = Color_Red;
            }
            else if (versionDate == 1)
            {

                versionText.Text += " (WOW MOD DEV)";
                versionText.Foreground = Color_Purple;
                expanderButton.Foreground = Color_Purple;
            }
            else if (ShaMismatch)
            {
                versionText.Text += $" (Modified)";
                versionText.Foreground = Color_Yellow;
                expanderButton.Foreground = Color_Yellow;
            }
            expanderContent.Children.Add(versionText);
            if (webMod.HomePage.IsValidUrl())
            {
                Button homepageButton = new()
                {
                    Content = "Homepage",
                    Margin = new(0, 5, 0, 5),
                    Tag = webMod.HomePage
                };
                homepageButton.Click += RoutedOpenURL;
                expanderContent.Children.Add(homepageButton);
            }
            if (webMod.DependentMods.Length != 0)
            {
                StackPanel dependencyBlock = new();
                dependencyBlock.Children.Add(new TextBlock { Text = $"Dependencies:" });
                foreach (var githubPath in webMod.DependentMods)
                {
                    string modName;
                    WebModInfo? temp = WebModsList.Find(x => x.DownloadLink == githubPath);
                    modName = temp == null ? Path.GetFileName(githubPath) : temp.Name;
                    TextBlock modBlock = new()
                    {
                        Text = modName
                    };
                    if (!LocalModsList.Any(x => x.Name == modName))
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
                Margin = new(0, 0, 10, 0)
            };
            modGrid.Children.Add(controlsPanel);

            CheckBox isEnabledBox = new CheckBox()
            {
                IsChecked = !localMod.Disabled,
                Content = "On",
                Tag = webMod.Name,
                Foreground = "#ddd".ToBrush()
            };
            isEnabledBox.Checked += ModCheckboxChanged;
            isEnabledBox.Unchecked += ModCheckboxChanged;
            controlsPanel.Children.Add(isEnabledBox);


            Button uninstallButton = new()
            {
                Content = "Uninstall",
                Width = 75,
                Height = 30,
                Tag = webMod.Name,
                Margin = new(30, LazyMarginLoL, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = Color_BG50,
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
                    Margin = new(30, LazyMarginLoL, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = Color_BG50,
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

        //Adds a mod that isn't tracked online
        public void AddMod(LocalModInfo localMod)
        {
            Grid modGrid = new()
            {
                Tag = localMod.Name
            };
            ModItemsContainer.Children.Add(modGrid);
            StackPanel expanderPanel = new()
            {
                Width = 675
            };
            modGrid.Children.Add(expanderPanel);

            Button expanderButton = new()
            {
                Width = 675,
                Height = 75,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = Color_BBB,
                Content = localMod.Name,
                Margin = new(0, 15, 0, 0),
                BorderBrush = Brushes.Transparent
            };
            expanderButton.Click += Button_Expander;
            expanderPanel.Children.Add(expanderButton);

            StackPanel expanderContent = new()
            {
                Tag = "ExpanderContent",
                IsVisible = false,
                Margin = new(50, 0, 0, 0)
            };
            expanderPanel.Children.Add(expanderContent);
            expanderContent.Children.Add(new TextBlock
            {
                Text = $"{((localMod.Description == "" || localMod.Description == null) ? "" : localMod.Description)}\n\nAuthor: {localMod.Author}\nVersion:\n",
            });
            expanderContent.Children.Add(new TextBlock
            {
                Text = $"{localMod.Version}",
            });
            if (localMod.HomePage.IsValidUrl())
            {
                Button homepageButton = new()
                {
                    Content = "Homepage",
                    Margin = new(0, 5, 0, 5),
                    Tag = localMod.HomePage
                };
                homepageButton.Click += RoutedOpenURL;
                expanderContent.Children.Add(homepageButton);
            }




            StackPanel controlsPanel = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new(0, 0, 10, 0)
            };
            modGrid.Children.Add(controlsPanel);


            Button downloadButton = new()
            {
                Content = "Uninstall",
                Width = 75,
                Height = 30,
                Tag = localMod.Name,
                Margin = new(300, LazyMarginLoL, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = Color_BG50,
            };
            downloadButton.Click += UninstallMod;
            controlsPanel.Children.Add(downloadButton);
        }

        //Adds a mod that isn't installed
        public void AddMod(WebModInfo webMod, int index = -1)
        {
            Grid modGrid = new()
            {
                Tag = webMod.Name
            };
            StackPanel expanderPanel = new()
            {
                Width = 675
            };
            modGrid.Children.Add(expanderPanel);

            Button expanderButton = new()
            {
                Width = 675,
                Height = 75,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = Color_BBB,
                Content = webMod.Name,
                Margin = new(0, 15, 0, 0),
                BorderBrush = Brushes.Transparent
            };
            expanderButton.Click += Button_Expander;
            expanderPanel.Children.Add(expanderButton);

            StackPanel expanderContent = new()
            {
                Tag = "ExpanderContent",
                IsVisible = false,
                Margin = new(50,0,0,0)
            };
            expanderPanel.Children.Add(expanderContent);
            expanderContent.Children.Add(new TextBlock
            {
                Text = $"{webMod.Description}\n\nAuthor: {webMod.Author}\nVersion:\n",
            });
            expanderContent.Children.Add(new TextBlock
            {
                Text = $"{webMod.Version}",
            });
            if (webMod.HomePage.IsValidUrl())
            {
                Button homepageButton = new()
                {
                    Content = "Homepage",
                    Margin = new(0, 5, 0, 5),
                    Tag = webMod.HomePage
                };
                homepageButton.Click += RoutedOpenURL;
                expanderContent.Children.Add(homepageButton);
            }
            if (webMod.DependentMods.Length != 0)
            {
                StackPanel dependencyBlock = new();
                dependencyBlock.Children.Add(new TextBlock { Text = $"Dependencies:" });
                foreach (var githubPath in webMod.DependentMods)
                {
                    string modName;
                    WebModInfo? temp = WebModsList.Find(x => x.DownloadLink == githubPath);
                    modName = temp == null ? Path.GetFileName(githubPath) : temp.Name;
                    TextBlock modBlock = new()
                    {
                        Text = modName
                    };
                    if (!LocalModsList.Any(x => x.Name == modName))
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
                Margin = new(0,0,10,0)
            };
            modGrid.Children.Add(controlsPanel);


            Button downloadButton = new()
            {
                Content = "Install",
                Width = 75,
                Height = 30,
                Tag = webMod.Name,
                Margin = new(300, LazyMarginLoL, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = Color_BG50,
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


        public void UpdateModDisplay(WebModInfo webMod, LocalModInfo localMod)
        {
            for (int i = 0; i < ModItemsContainer.Children.Count; i++)
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
            int webModIdx = WebModsList.FindIndex(x => x.Name == localMod.Name);
            if (webModIdx == -1)
            {
                ModItemsContainer.Children.Remove(ModItemsContainer.Children.First(x => (string)((Control)x).Tag == localMod.Name));
                return;
            }
            var webMod = WebModsList[webModIdx];
            for (int i = 0; i < ModItemsContainer.Children.Count; i++)
            {

                if ((string)((Control)ModItemsContainer.Children[i]).Tag == webMod.Name)
                {
                    AddMod(webMod, i);
                    break;
                }
            }
        }
        public LocalModInfo LoadLocalMod(string file)
        {
            string tempPath = Path.GetFileName(file);
            if (tempPath.EndsWith(".disabled"))
            {
                tempPath = tempPath[..^9];
            }
            var mod = new LocalModInfo()
            {
                Disabled = file.EndsWith(".disabled"),
                FileName = tempPath
            };
            var assembly = Assembly.Load(File.ReadAllBytes(file));
            var properties = assembly.GetCustomAttribute(typeof(MelonInfoAttribute)).GetType().GetProperties();
            foreach (var property in properties)
            {
                var temp = property.GetValue(assembly.GetCustomAttribute(typeof(MelonInfoAttribute)), null);
                switch (property.Name)
                {
                    case "Name":
                        if (temp == null)
                            return null;
                        mod.Name = temp.ToString();
                        break;
                    case "Version":
                        if (temp == null)
                            return null;
                        mod.Version = temp.ToString();
                        break;
                    case "Description":
                        if (temp == null)
                            continue;
                        mod.Description = temp.ToString();
                        break;
                    case "Author":
                        if (temp == null)
                            continue;
                        mod.Author = temp.ToString();
                        break;
                    case "DownloadLink":
                        if (temp == null)
                            continue;
                        mod.HomePage = temp.ToString();
                        if (!mod.HomePage.IsValidUrl())
                        {
                            mod.HomePage = null;
                        }
                        break;
                    default:
                        break;
                };
            } 
            SHA256 mySHA256 = SHA256.Create();
            FileStream fileStream = File.OpenRead(file);
            byte[] hashValue = mySHA256.ComputeHash(fileStream);
            string output = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            mod.SHA256 = output;
            mySHA256.Dispose();
            fileStream.Close();
            return mod;
        }

        //Loads installed mods from the mods folder
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
                
                string path = Path.Join(CurrentGameDirectory, "Mods");
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
                            catch (Exception) {}
                            continue;
                        }
                        var localMod = LoadLocalMod(file);
                        if (localMod != null)
                        {
                            LocalModsList.Add(localMod);
                        }

                    }
                    catch (Exception) { }
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
            string searchTerm = ((TextBox)sender).Text;
            if (searchTerm == null || searchTerm == "")
            {
                foreach (Control control in ModItemsContainer.Children)
                {
                    control.IsVisible = true;
                }
                return;
            }

            searchTerm = Regex.Replace(searchTerm, "[ ]{2,}", " ", RegexOptions.None);
            bool any = false;
            foreach (Control control in ModItemsContainer.Children)
            {
                string modName = (string)control.Tag;
                control.IsVisible = false;
                LocalModInfo localMod = LocalModsList.Find(x => x.Name == modName);
                WebModInfo webMod = WebModsList.Find(x => x.Name == modName);
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
                string modName = (string)control.Tag;
                control.IsVisible = false;
                LocalModInfo localMod = LocalModsList.Find(x => x.Name == modName);
                WebModInfo webMod = WebModsList.Find(x => x.Name == modName);
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
            bool? isChecked = ((ToggleButton)sender).IsChecked;
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
            byte[] data = GithubFileDownload(webMod.DownloadLink);
            if (data == null)
            {
                DialogPopup("Mod download failed\n(are you online?)");
                return false;
            }
            int localModIdx = LocalModsList.FindIndex(x => x.Name == webMod.Name);
            string path = Path.Join(CurrentGameDirectory, "Mods", localModIdx == -1 ? webMod.DownloadLink[5..] : LocalModsList[localModIdx].FileName + (LocalModsList[localModIdx].Disabled ? ".disabled" : ""));
            int lastIdx = path.LastIndexOf(Path.DirectorySeparatorChar);
            foreach (string dependencyName in webMod.DependentMods)
            {
                var findMod = WebModsList.Find(x => x.DownloadLink == dependencyName);
                if (!InstallModUpdate(findMod?.Name, false))
                {
                    return false;
                }
            }
            if (localModIdx != -1)
            {
                try
                {
                    File.WriteAllBytes(path, data);
                    var localMod = LoadLocalMod(path);
                    LocalModsList.Add(localMod);
                    UpdateModDisplay(webMod, localMod);
                    if (SuccessPopups && allowSuccessSetting)
                    {
                        DialogPopup("Update successful.");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case SecurityException:
                        case UnauthorizedAccessException:
                            DialogPopup("File update failed\n(unauthorized)");
                            break;
                        case IOException:
                            DialogPopup("File update failed\n(is the game running?)");
                            break;
                        default:
                            DialogPopup($"File update failed\n({ex.GetType()})");
                            break;
                    }
                    return false;
                }
            }
            else
            {
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
        }

        public void InstallModUpdateCall(object? sender, RoutedEventArgs args)
        {
            InstallModUpdate((string)((Control)sender).Tag, true);
        }

        public void UninstallMod(object? sender, RoutedEventArgs args)
        {
            var localMod = LocalModsList.Find(x => x.Name == (string)((Control)sender).Tag);

            string path = Path.Join(CurrentGameDirectory, "Mods", localMod.FileName + (localMod.Disabled ? ".disabled" : ""));
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
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? linkToOpen : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "open"),
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {linkToOpen}" : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? linkToOpen : ""),
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
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? pathToOpen : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "open"),
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-R {pathToOpen}" : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? pathToOpen : ""),
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

    public class WebModInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string DownloadLink { get; set; }
        public string HomePage { get; set; }
        public string[] GameVersion { get; set; }
        public string Description { get; set; }
        public string[] DependentMods { get; set; }
        public string[] DependentLibs { get; set; }
        public string[] IncompatibleMods { get; set; }
        public string SHA256 { get; set; }
    }

    public class LocalModInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string SHA256 { get; set; }
        public string FileName { get; set; }
        public bool Disabled { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? HomePage { get; set; }
    }
}
