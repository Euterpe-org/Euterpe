namespace Euterpe.Tests.App.ViewModels;

public sealed partial class ShareImportDialogViewModelTest
{
    [Test]
    public async Task Prepare_ValidPackage_EnablesImportAndShowsContents()
    {
        var package = CreatePackage(GameId.MuseDash);
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("valid").Returns(package);
        var viewModel = CreateViewModel(shareService);

        viewModel.Prepare("valid");

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.CanImport).IsTrue();
        await Assert.That(viewModel.ShareText).IsEqualTo("valid");
        await Assert.That(viewModel.ValidationMessage).IsNotNull();
    }

    [Test]
    public async Task Prepare_PackageForAnotherGame_DisablesImport()
    {
        var package = CreatePackage(GameId.MuseDash2);
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("wrong-game").Returns(package);
        var viewModel = CreateViewModel(shareService);

        viewModel.Prepare("wrong-game");

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.CanImport).IsFalse();
        await Assert.That(viewModel.ValidationMessage).IsNotNull();
    }
}
