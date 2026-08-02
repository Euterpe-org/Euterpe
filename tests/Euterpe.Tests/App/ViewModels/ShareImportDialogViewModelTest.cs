using Euterpe.Features.Share;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.App.ViewModels;

[Category("ShareImportDialogViewModelTests")]
[TestSubject(typeof(ShareImportDialogViewModel))]
public sealed partial class ShareImportDialogViewModelTest
{
    private static ShareImportDialogViewModel CreateViewModel(IGameShareService shareService) =>
        new()
        {
            Launcher = IPlatformLauncher.Mock(),
            GameShareService = shareService,
            Config = new Config(),
            Logger = NullLogger<ShareImportDialogViewModel>.Instance
        };

    private static GameSharePackage CreatePackage(GameId gameId) =>
        new()
        {
            GameId = gameId,
            ChartIds = [13]
        };
}
