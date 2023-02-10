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

namespace MuseDashModToolsUI.Views
{

#pragma warning disable SYSLIB0014
#pragma warning disable CS8618
    public partial class MainWindow : Window
    {
        public static MainWindow _Instance { get; private protected set; } = null;
        public int Selected_ModFilter { get; set; } = 0;
        public Version GameVersion { get; private set; } = null;

        public List<WebModInfo> WebModsList { get; private set; }
        public List<LocalModInfo> LocalModsList { get; private set; } = new();

        private const string ModLinks = "MDModsDev/ModLinks/dev/ModLinks.json";
        public MainWindow()
        {
            InitializeComponent();
            _Instance = this;
            InitializeWebModsList();
            InitializeLocalModsList();
            FinishInitialization();

        }

        public void ExitProgram()
        {
            Environment.Exit(0);
        }

        /*Completely erases all contents of the main window and opens a dialog with the given errorMessage
         *Once the dialog is exited, the program will close.*/
        public void ErrorAndExit(string errorMessage)
        {
            DialogWindow dialog = new();
            this.Content = null;
            dialog.TextDisplay = errorMessage;
            dialog.ButtonClickFunction = ExitProgram;
            this.Show();
            dialog.ShowDialog(this);
        }

        /* The following functions are shorthands for opening a new DialogWindow,
         * and then returning the created window */
        public DialogWindow DialogPopup(string message, Action exitAction, bool isModal = true)
        {
            DialogWindow dialog = new();
            dialog.TextDisplay = message;
            dialog.ButtonClickFunction = exitAction;
            if (isModal)
            {
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
            DialogWindow dialog = new();
            dialog.TextDisplay = message;
            if (isModal)
            {
                dialog.ShowDialog(this);
            }
            else
            {
                dialog.Show();
            }
            return dialog;
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
                    data = webClient.DownloadString("https://raw.githubusercontent.com/" + ModLinks);
                }
                catch (WebException)
                {
                    data = webClient.DownloadString("https://raw.fastgit.org/" + ModLinks);
                }
                webClient.Dispose();
                WebModsList = JsonSerializer.Deserialize<List<WebModInfo>>(data);
            }
            catch (Exception)
            {
                ErrorAndExit("Failed to download\nonline mod info");
            }
        }

        //Main function is to compare mods and call each AddMod function appropriately
        public void FinishInitialization()
        {
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

        //Adds a mod that's installed
        public void AddMod(WebModInfo webMod, LocalModInfo localMod)
        {
            
        }

        //Adds a mod that isn't tracked online
        public void AddMod(LocalModInfo localMod)
        {

        }

        //Adds a mod that isn't installed
        public void AddMod(WebModInfo webMod)
        {
            Grid modGrid = new();
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
                VerticalContentAlignment = VerticalAlignment.Top,
                Foreground = (IBrush)new BrushConverter().ConvertFromString("#bbb"),
                Content = webMod.Name,
                Margin = new(0,15,0,0)
            };
            expanderPanel.Children.Add(expanderButton);

            StackPanel expanderContent = new()
            {
                Tag = "ExpanderContent",
                IsVisible = false,
                Margin = new(30,0,0,0)
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
            if (webMod.HomePage != "")
            {
                Button homepageButton = new()
                {
                    Content = "Homepage",
                    Margin = new(0, 0, 0, 10),
                    Tag = webMod.HomePage
                };
                homepageButton.Click += new EventHandler<RoutedEventArgs>(RoutedOpenURL);
                expanderContent.Children.Add(homepageButton);
            }
            expanderContent.Children.Add(new TextBlock
            {
                Text = $"Dependencies:\n{string.Join("\n", webMod.DependentMods)}",
            });




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
                Margin = new(250, 30, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };
            downloadButton.Click += InstallModUpdate;
            controlsPanel.Children.Add(downloadButton);




        }

        public void UpdateModDisplay(string modName)
        {

        }

        //Loads installed mods from the mods folder
        public void InitializeLocalModsList()
        {
            try
            {
                
                string path = Path.Join(Directory.GetCurrentDirectory(), "Mods");
                var files = Directory.GetFiles(path, "*.dll")
                    .Concat(Directory.GetFiles(path, "*.dll.disabled"))
                    .ToList();
                foreach (var file in files)
                {
                    try
                    {
                        bool isDisabled = file.EndsWith(".disabled");
                        if (isDisabled && files.Contains(file[..^9]))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception) {}
                            continue;
                        }
                        var mod = new LocalModInfo() { Disabled = isDisabled };
                        var assembly = Assembly.LoadFrom(file);
                        var properties = assembly.GetCustomAttribute(typeof(MelonInfoAttribute)).GetType().GetProperties();
                        foreach (var property in properties)
                        {
                            if (property.Name == "Name")
                            {
                                mod.Name = property.GetValue(assembly.GetCustomAttribute(typeof(MelonInfoAttribute)), null).ToString();
                            }
                            if (property.Name == "Version")
                            {
                                mod.Version = property.GetValue(assembly.GetCustomAttribute(typeof(MelonInfoAttribute)), null).ToString();
                            }
                        }
                        SHA256 mySHA256 = SHA256.Create();
                        var fInfo = new FileInfo(file);
                        FileStream fileStream = fInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fileStream.Position = 0;
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);
                        string output = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                        mod.SHA256 = output;
                        fileStream.Close();
                        LocalModsList.Add(mod);
                    }
                    catch (Exception) { throw; }
                }
            }
            catch (Exception)
            {
                ErrorAndExit("Failed to read local mods\nMake sure you're in the game directory");
                throw;
            }
                
        }

        public void UpdateFilters()
        {

        }

        //Used for implementing expanders, cause the built-in one is glitchy as all hell.
        public void Button_Expander(object sender, RoutedEventArgs args)
        {
            ((Panel)((Control)sender).Parent)
                .Children
                .First(x => 
                    ((Control)x).Tag == "ExpanderContent"
                )
                .IsVisible ^= true;
            File.WriteAllText("asd.txt", "heheheha");
        }

        public void RoutedOpenURL(object? sender, RoutedEventArgs args)
        {
            OpenURL((string)((Control)sender).Tag);
        }

        public void InstallModUpdate(object? sender, RoutedEventArgs args)
        {
        }

        public void OpenURL(string linkToOpen)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? linkToOpen : "open",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {linkToOpen}" : "",
                    CreateNoWindow = true,
                    UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                });
            }
            catch (Exception) { }
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
        public bool Disabled { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? HomePage { get; set; }
    }
}
