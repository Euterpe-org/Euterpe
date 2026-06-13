namespace Euterpe.Features.Charting;

public sealed partial class DifficultyEditViewModel : ObservableObject
{
    public DifficultyEditViewModel(ChartDifficulty difficulty, string rating, string chartersText)
    {
        Difficulty = difficulty;
        Rating = rating;
        ChartersText = chartersText;
    }

    public ChartDifficulty Difficulty { get; }

    public LocalizedString DifficultyName => $"ChartDifficulty_{Difficulty.ToStringFast()}";

    [ObservableProperty]
    public partial string Rating { get; set; }

    [ObservableProperty]
    public partial string ChartersText { get; set; }
}
