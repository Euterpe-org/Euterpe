namespace Euterpe.Headless.Tests.Controls;

[TestSubject(typeof(Difficulty))]
public sealed class DifficultyTests : HeadlessTest
{
    [Test]
    public Task Text_BindsToTemplateTextBlock() => RunOnUI(async () =>
    {
        var difficulty = new Difficulty { Text = "Hard" };
        var window = new Window { Content = difficulty, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var textBlock = difficulty.GetVisualDescendants().OfType<TextBlock>().Single();
        await Assert.That(textBlock.Text).IsEqualTo("Hard");
    });

    [Test]
    public Task TextChange_PropagatesToTemplateTextBlock() => RunOnUI(async () =>
    {
        var difficulty = new Difficulty { Text = "Easy" };
        var window = new Window { Content = difficulty, Width = 200, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        difficulty.Text = "Master";
        Dispatcher.UIThread.RunJobs();

        var textBlock = difficulty.GetVisualDescendants().OfType<TextBlock>().Single();
        await Assert.That(textBlock.Text).IsEqualTo("Master");
    });
}