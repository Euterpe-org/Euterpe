using System.Diagnostics.CodeAnalysis;

namespace Euterpe.Models.Games;

public abstract partial class GameConfig : ObservableObject
{
    [AllowNull]
    [ObservableProperty]
    public partial string Folder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial GameMode GameMode { get; set; } = GameMode.Modded;

    public bool SetupCompleted { get; set; }
}