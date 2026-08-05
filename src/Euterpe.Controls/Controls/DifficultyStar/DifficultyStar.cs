using Euterpe.Models.Charts;

namespace Euterpe.Controls;

public sealed class DifficultyStar : TemplatedControl
{
    public static readonly StyledProperty<ChartDifficulty> DifficultyProperty =
        AvaloniaProperty.Register<DifficultyStar, ChartDifficulty>(nameof(Difficulty), ChartDifficulty.Easy);

    public DifficultyStar() => UpdatePseudoClasses(Difficulty);

    public ChartDifficulty Difficulty
    {
        get => GetValue(DifficultyProperty);
        set => SetValue(DifficultyProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DifficultyProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<ChartDifficulty>());
        }
    }

    private void UpdatePseudoClasses(ChartDifficulty difficulty)
    {
        PseudoClasses.Set(":easy", difficulty is ChartDifficulty.Easy);
        PseudoClasses.Set(":hard", difficulty is ChartDifficulty.Hard);
        PseudoClasses.Set(":master", difficulty is ChartDifficulty.Master);
        PseudoClasses.Set(":hidden", difficulty is ChartDifficulty.Hidden);
    }
}
