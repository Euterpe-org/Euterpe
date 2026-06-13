namespace Euterpe.Models.Playback;

public sealed partial class PlaybackState : ObservableObject
{
    [ObservableProperty]
    public partial string? PlayingKey { get; private set; }

    [ObservableProperty]
    public partial PlaybackStatus Status { get; private set; }

    public void Set(PlaybackStatus status, string? playingKey)
    {
        PlayingKey = playingKey;
        Status = status;
    }
}
