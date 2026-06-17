namespace Euterpe.Features.Setting;

// ReSharper disable StringLiteralTypo
[Route("/setting/about", DisplayName = Panel_Setting_About, Order = 0)]
public sealed partial class AboutPanelViewModel : ViewModelBase
{
    public ContributorGroup[] ContributorGroups { get; } =
    [
        // Developer Team
        new(Contributor_Developer, [
            new Contributor("lxy",
                "Planning and maintaining the project",
                [
                    ("GitHub", "https://github.com/lxymahatma"),
                    ("Bilibili", "https://space.bilibili.com/255895683")
                ]),
            new Contributor("KARPED1EM",
                "Remaking UI",
                [
                    ("GitHub", "https://github.com/KARPED1EM"),
                    ("Bilibili", "https://space.bilibili.com/312252452")
                ]),
            new Contributor("Balint",
                "Making the first version of the project",
                [("GitHub", "https://github.com/Balint817")]),
            new Contributor("Ultra Rabbit",
                "Rewriting the first version of the project",
                [("GitHub", "https://github.com/TheBunnies")])
        ]),

        // Artist
        new(Contributor_Artist, [
            new Contributor("Super Pig",
                "Drawing the euterpe application icon",
                [("Bilibili", "https://space.bilibili.com/252615263")]),
            new Contributor("aquawtf",
                "Drawing the euterpe icon"),
            new Contributor("Bigbeesushi",
                "Drawing the euterpe background",
                [("YouTube", "https://www.youtube.com/@%E9%AD%94%E6%B3%95%E5%B8%AB%E7%8E%A5%E6%9C%88")])
        ]),

        // Translators
        new(Translator_ChineseSimplified, [
            new Contributor("lxymahatma")
        ]),
        new(Translator_ChineseTraditional, [
            new Contributor("Shiron Lee"),
            new Contributor("Bigbeesushi")
        ]),
        new(Translator_Hungarian, [
            new Contributor("Balint")
        ]),
        new(Translator_Korean, [
            new Contributor("MEMOLie")
        ]),
        new(Translator_Russian, [
            new Contributor("Ultra Rabbit"),
            new Contributor("Ronner"),
            new Contributor("taypexx")
        ]),
        new(Translator_Spanish, [
            new Contributor("MNight4")
        ])
    ];

    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        var hasUpdate = await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
        if (!hasUpdate)
        {
            await MessageBoxService.SuccessAsync(MessageBox_Content_NoUpdatesFound).ConfigureAwait(false);
        }
    }

    #region Injections

    public required IUpdateService UpdateService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
