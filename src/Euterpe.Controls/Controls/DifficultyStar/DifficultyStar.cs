using Euterpe.Models.Charts;

namespace Euterpe.Controls;

public sealed class DifficultyStar : TemplatedControl
{
    public static readonly StyledProperty<ChartDifficulty> DifficultyProperty =
        AvaloniaProperty.Register<DifficultyStar, ChartDifficulty>(nameof(Difficulty), ChartDifficulty.Easy);

    public ChartDifficulty Difficulty
    {
        get => GetValue(DifficultyProperty);
        set => SetValue(DifficultyProperty, value);
    }

    static DifficultyStar()
    {
        DifficultyProperty.Changed.AddClassHandler<DifficultyStar>(static (star, change) =>
            star.UpdatePseudoClasses(change.GetNewValue<ChartDifficulty>()));
    }

    public DifficultyStar() => UpdatePseudoClasses(Difficulty);

    private void UpdatePseudoClasses(ChartDifficulty difficulty)
    {
        PseudoClasses.Set(":easy", difficulty is ChartDifficulty.Easy);
        PseudoClasses.Set(":hard", difficulty is ChartDifficulty.Hard);
        PseudoClasses.Set(":master", difficulty is ChartDifficulty.Master);
        PseudoClasses.Set(":hidden", difficulty is ChartDifficulty.Hidden);
    }
}
