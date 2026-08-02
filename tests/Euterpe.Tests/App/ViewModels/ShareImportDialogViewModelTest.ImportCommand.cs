using Euterpe.Models.Progress;

namespace Euterpe.Tests.App.ViewModels;

public sealed partial class ShareImportDialogViewModelTest
{
    [Test]
    public async Task ImportCommand_ValidPackage_DelegatesAndShowsSummary()
    {
        var package = CreatePackage(GameId.MuseDash);
        BulkItemResult[] result = [new("13", BulkItemOutcome.Added)];
        var shareService = IGameShareService.Mock();
        shareService.TryParseShareLink("valid").Returns(package);
        shareService.ImportAsync(package, Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).Returns(result);
        var viewModel = CreateViewModel(shareService);
        viewModel.Prepare("valid");

        await viewModel.ImportCommand.ExecuteAsync(null);

        using var _ = Assert.Multiple();
        shareService.ImportAsync(package, Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).WasCalled(Times.Once);
        await Assert.That(viewModel.IsImporting).IsFalse();
        await Assert.That(viewModel.ResultSummary).IsNotNull();
    }

    [Test]
    public async Task ImportCommand_Canceled_StopsImportAndShowsCanceledResult()
    {
        var package = CreatePackage(GameId.MuseDash);
        var viewModel = CreateViewModel(new CancelableGameShareService(package));
        viewModel.Prepare("valid");

        var importTask = viewModel.ImportCommand.ExecuteAsync(null);
        viewModel.CancelImport();
        await importTask;

        using var _ = Assert.Multiple();
        await Assert.That(viewModel.IsImporting).IsFalse();
        await Assert.That(viewModel.ResultSummary).IsNotNull();
    }

    private sealed class CancelableGameShareService(GameSharePackage package) : IGameShareService
    {
        public string CreateChartShareLink(IReadOnlyCollection<int> chartIds) => throw new NotSupportedException();

        public GameSharePackage? TryParseShareLink(string text) => package;

        public async Task<IReadOnlyList<BulkItemResult>> ImportAsync(GameSharePackage value, IProgress<BatchProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            return null!;
        }
    }
}
