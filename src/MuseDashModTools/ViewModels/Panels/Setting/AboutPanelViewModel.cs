using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace MuseDashModTools.ViewModels.Panels.Setting;

// ReSharper disable StringLiteralTypo
public sealed partial class AboutPanelViewModel : ViewModelBase
{
    private static readonly Dictionary<string, ContributorInfo> Contributors = new()
    {
        ["lxy"] = new ContributorInfo(
            "For planning and maintaining the project",
            [
                ("Bilibili", "https://space.bilibili.com/255895683"),
                ("Github", "https://github.com/lxymahatma"),
                ("Twitch", "https://www.twitch.tv/lxymahatma")
            ]),

        ["KARPED1EM"] = new ContributorInfo(
            "", // TODO: write desc for KARPED1EM
            [
                ("Github", "https://github.com/KARPED1EM"),
                ("Bilibili", "https://space.bilibili.com/312252452")
            ]),

        ["Balint"] = new ContributorInfo(
            "For making the first version of the project",
            [("Github", "https://github.com/Balint817")]
        ),

        ["Ultra Rabbit"] = new ContributorInfo(
            "For rewriting the first version of the project",
            [("Github", "https://github.com/TheBunnies")]
        ),

        ["Super Pig"] = new ContributorInfo(
            "For redrawing the Muse Dash Mod Tools icon",
            [("Bilibili", "https://space.bilibili.com/252615263")]
        ),

        ["Shiron_Lee"] = new ContributorInfo(),
        ["Bigbeesushi"] = new ContributorInfo(),
        ["MEMOLie"] = new ContributorInfo(),
        ["Ronner"] = new ContributorInfo(),
        ["taypexx"] = new ContributorInfo(),
        ["MNight4"] = new ContributorInfo()
    };

    private static readonly (string GroupName, string[] Members)[] Groups =
    [
        (XAML_Developer, ["lxy", "KARPED1EM", "Balint", "Ultra Rabbit"]),
        (XAML_Artist, ["Super Pig"]),
        (XAML_ChineseTraditional, ["Shiron_Lee", "Bigbeesushi"]),
        (XAML_Hungarian, ["Balint"]),
        (XAML_Korean, ["MEMOLie"]),
        (XAML_Russian, ["Ultra Rabbit", "Ronner", "taypexx"]),
        (XAML_Spanish, ["MNight4"])
    ];

    public ObservableCollection<ContributorGroup> ContributorGroups => new(
        Groups.Select(g => new ContributorGroup(
            g.GroupName,
            new ObservableCollection<ContributorCardItem>(
                g.Members.Select(m => CreateCard(m, Contributors[m]))
            )
        ))
    );

    private ContributorCardItem CreateCard(string name, ContributorInfo info)
    {
        var links = info.Links?.Length > 0
            ? new ObservableCollection<ContributorCardLinkItem>(
                info.Links.Select(l => new ContributorCardLinkItem(l.Type, l.Url)))
            : null;

        var avatar = ResourceService.TryGetAppResource<Bitmap>(
            $"avares://MuseDashModTools/Assets/Contributors/{name.Replace(' ', '_')}.webp");

        return new ContributorCardItem(name, info.Description, avatar, links);
    }

    private sealed record ContributorInfo(
        string? Description = null,
        (string Type, string Url)[]? Links = null);

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

public record ContributorGroup(string GroupName, ObservableCollection<ContributorCardItem> Contributors);