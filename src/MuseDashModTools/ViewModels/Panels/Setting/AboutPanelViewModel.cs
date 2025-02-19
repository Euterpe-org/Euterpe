namespace MuseDashModTools.ViewModels.Panels.Setting;

// ReSharper disable StringLiteralTypo
public sealed partial class AboutPanelViewModel : ViewModelBase
{
    public class ContributorGroup(string name, List<ContributorCardItem> contributors)
    {
        public string Name { get; } = name;
        public List<ContributorCardItem> Contributors { get; } = contributors;
    }

    public List<ContributorGroup> ContributorGroups { get; } =
    [
        // Core Team
        new(XAML_Developer, [
            new ContributorCardItem("lxy",
                "For planning and maintaining the project",
                links:
                [
                    ("Bilibili", "https://space.bilibili.com/255895683"),
                    ("Github", "https://github.com/lxymahatma"),
                    ("Twitch", "https://www.twitch.tv/lxymahatma")
                ]),
            new ContributorCardItem("KARPED1EM",
                links:
                [
                    ("Github", "https://github.com/KARPED1EM"),
                    ("Bilibili", "https://space.bilibili.com/312252452")
                ]),
            new ContributorCardItem("Balint",
                "For making the first version of the project",
                links: [("Github", "https://github.com/Balint817")]),
            new ContributorCardItem("Ultra Rabbit",
                "For rewriting the first version of the project",
                links: [("Github", "https://github.com/TheBunnies")])
        ]),
        new(XAML_Artist, [
            new ContributorCardItem("Super Pig",
                "For redrawing the Muse Dash Mod Tools icon",
                links: [("Bilibili", "https://space.bilibili.com/252615263")])
        ]),
        // Translators
        new(XAML_ChineseTraditional, [
            new ContributorCardItem("Shiron_Lee"),
            new ContributorCardItem("Bigbeesushi")
        ]),
        new(XAML_Hungarian, [
            new ContributorCardItem("Balint",
                links: [("Github", "https://github.com/Balint817")])
        ]),
        new(XAML_Korean, [
            new ContributorCardItem("MEMOLie")
        ]),
        new(XAML_Russian, [
            new ContributorCardItem("Ultra Rabbit",
                links: [("Github", "https://github.com/TheBunnies")]),
            new ContributorCardItem("Ronner"),
            new ContributorCardItem("taypexx")
        ]),
        new(XAML_Spanish, [
            new ContributorCardItem("MNight4")
        ])
    ];

    [RelayCommand]
    private Task OpenUrl(string url) => PlatformService.OpenUriAsync(url);

    [RelayCommand]
    private void CheckUpdate()
    {
    }

    #region Injections

    [UsedImplicitly]
    public IResourceService ResourceService { get; set; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ModsPanelViewModel> Logger { get; init; } = null!;

    #endregion Injections
}