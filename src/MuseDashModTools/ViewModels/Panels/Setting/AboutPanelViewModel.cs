namespace MuseDashModTools.ViewModels.Panels.Setting;

// ReSharper disable StringLiteralTypo
public sealed partial class AboutPanelViewModel : ViewModelBase
{
    public ContributorGroup[] ContributorGroups { get; } =
    [
        // Developer Team
        new(XAML_Contributor_Developer, [
            new ContributorCardItem("lxy",
                "Planning and maintaining the project",
                [
                    ("GitHub", "https://github.com/lxymahatma"),
                    ("Bilibili", "https://space.bilibili.com/255895683")
                ]),
            new ContributorCardItem("KARPED1EM",
                "Remaking UI",
                [
                    ("GitHub", "https://github.com/KARPED1EM"),
                    ("Bilibili", "https://space.bilibili.com/312252452")
                ]),
            new ContributorCardItem("Balint",
                "Making the first version of the project",
                [("GitHub", "https://github.com/Balint817")]),
            new ContributorCardItem("Ultra Rabbit",
                "Rewriting the first version of the project",
                [("GitHub", "https://github.com/TheBunnies")])
        ]),

        // Artist
        new(XAML_Contributor_Artist, [
            new ContributorCardItem("Super Pig",
                "Drawing the MDMT application icon",
                [("Bilibili", "https://space.bilibili.com/252615263")]),
            new ContributorCardItem("aquawtf",
                "Drawing the MDMT icon"),
            new ContributorCardItem("Bigbeesushi",
                "Drawing the MDMT background",
                [("YouTube", "https://www.youtube.com/@%E9%AD%94%E6%B3%95%E5%B8%AB%E7%8E%A5%E6%9C%88")])
        ]),

        // Translators
        new(XAML_Translator_ChineseSimplified, [
            new ContributorCardItem("lxymahatma")
        ]),
        new(XAML_Translator_ChineseTraditional, [
            new ContributorCardItem("Shiron Lee"),
            new ContributorCardItem("Bigbeesushi")
        ]),
        new(XAML_Translator_Hungarian, [
            new ContributorCardItem("Balint")
        ]),
        new(XAML_Translator_Korean, [
            new ContributorCardItem("MEMOLie")
        ]),
        new(XAML_Translator_Russian, [
            new ContributorCardItem("Ultra Rabbit"),
            new ContributorCardItem("Ronner"),
            new ContributorCardItem("taypexx")
        ]),
        new(XAML_Translator_Spanish, [
            new ContributorCardItem("MNight4")
        ])
    ];

    #region Injections

    [UsedImplicitly]
    public IUpdateService UpdateService { get; init; } = null!;

    #endregion Injections

    [RelayCommand]
    private Task CheckUpdateAsync() => UpdateService.CheckForUpdatesAsync();
}