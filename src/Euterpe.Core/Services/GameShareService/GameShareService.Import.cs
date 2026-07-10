namespace Euterpe.Core;

internal sealed partial class GameShareService
{
    private static IProgress<BatchProgress>? CreatePhaseProgress(
        IProgress<BatchProgress>? progress,
        int completedBeforePhase,
        int total) =>
        progress is null ? null : new PhaseProgress(progress, completedBeforePhase, total);

    private sealed class PhaseProgress(
        IProgress<BatchProgress> progress,
        int completedBeforePhase,
        int total) : IProgress<BatchProgress>
    {
        public void Report(BatchProgress value) =>
            progress.Report(new BatchProgress(completedBeforePhase + value.Completed, total));
    }
}
