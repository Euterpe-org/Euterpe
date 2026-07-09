using Euterpe.Features.Setting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("DownloadPanelViewModelTests")]
[TestSubject(typeof(DownloadPanelViewModel))]
public sealed class DownloadPanelViewModelTest
{
    [Test]
    [Arguments(0, UpdateChannel.Stable)]
    [Arguments(1, UpdateChannel.Beta)]
    public async Task SelectedUpdateChannelIndex_WritesBackToConfig(int index, UpdateChannel expected)
    {
        var config = NewConfig();
        var vm = NewViewModel(config);

        vm.SelectedUpdateChannelIndex = index;

        await Assert.That(config.UpdateChannel).IsEqualTo(expected);
    }

    [Test]
    public async Task SelectedUpdateChannelIndex_ChangeRaisesPropertyChanged()
    {
        var vm = NewViewModel(NewConfig());
        var changed = new List<string?>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        vm.SelectedUpdateChannelIndex = 1;

        await Assert.That(changed).Contains(nameof(DownloadPanelViewModel.SelectedUpdateChannelIndex));
    }

    private static Config NewConfig() =>
        new() { MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() };

    private static DownloadPanelViewModel NewViewModel(Config config) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<DownloadPanelViewModel>.Instance,
        Config = config
    };
}
