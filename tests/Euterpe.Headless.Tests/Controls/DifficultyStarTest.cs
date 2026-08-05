using Euterpe.Models.Charts;
using Path = Avalonia.Controls.Shapes.Path;

namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(DifficultyStar))]
[Category("DifficultyStarTests")]
public sealed class DifficultyStarTest : HeadlessTest
{
    private static DifficultyStar ShowStar(ChartDifficulty difficulty)
    {
        var star = new DifficultyStar { Difficulty = difficulty, Width = 48, Height = 48 };
        var window = new Window { Content = star, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return star;
    }

    private static IBrush? FillOf(DifficultyStar star) =>
        star.GetVisualDescendants().OfType<Path>().Single().Fill;

    [Test]
    [Arguments(ChartDifficulty.Easy)]
    [Arguments(ChartDifficulty.Hard)]
    [Arguments(ChartDifficulty.Master)]
    [Arguments(ChartDifficulty.Hidden)]
    public Task Difficulty_KnownMember_PaintsALaneBrush(ChartDifficulty difficulty) => RunOnUI(async () =>
        await Assert.That(FillOf(ShowStar(difficulty))).IsNotNull());

    [Test]
    public Task Difficulty_EveryMember_PaintsItsOwnBrush() => RunOnUI(async () =>
    {
        var fills = ChartDifficultyExtensions.GetValues().Select(static difficulty => FillOf(ShowStar(difficulty))).ToList();
        await Assert.That(fills).HasDistinctItems();
    });

    [Test]
    public Task Difficulty_Changed_RepaintsWithTheNewLaneBrush() => RunOnUI(async () =>
    {
        var star = ShowStar(ChartDifficulty.Easy);
        var easyFill = FillOf(star);

        star.Difficulty = ChartDifficulty.Master;
        Dispatcher.UIThread.RunJobs();

        await Assert.That(FillOf(star)).IsNotEqualTo(easyFill);
    });

    [Test]
    public Task Difficulty_OutsideTheEnum_KeepsTheFallbackFill() => RunOnUI(async () =>
        await Assert.That(FillOf(ShowStar((ChartDifficulty)0))).IsNotNull());
}
