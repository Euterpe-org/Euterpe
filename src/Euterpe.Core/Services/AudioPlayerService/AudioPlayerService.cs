using Avalonia.Threading;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Components;

namespace Euterpe.Core;

internal sealed partial class AudioPlayerService : IAudioPlayerService
{
    private AudioPlaybackDevice? _device;
    private CancellationTokenSource? _playCts;
    private SoundPlayer? _player;
    private EventHandler<EventArgs>? _playerEndedHandler;

    public async Task PlayAsync(string key, string filePath)
    {
        StopPlayer();
        PlaybackState.Set(PlaybackStatus.Playing, key);

        _playCts?.Cancel();
        var cts = _playCts = new CancellationTokenSource();

        try
        {
            var (source, format) = await Task.Run(() => Prepare(filePath), cts.Token).ConfigureAwait(false);
            Dispatcher.UIThread.Post(() => Activate(source, format, cts));
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer play or stopped mid-load; not a failure.
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"Failed to start audio playback for {filePath}");
            Dispatcher.UIThread.Post(() => Fail(cts));
        }
    }

    public void Pause()
    {
        _player?.Pause();
        if (PlaybackState.PlayingKey is { } key)
        {
            PlaybackState.Set(PlaybackStatus.Paused, key);
        }
    }

    public void Resume()
    {
        _player?.Play();
        if (PlaybackState.PlayingKey is { } key)
        {
            PlaybackState.Set(PlaybackStatus.Playing, key);
        }
    }

    public void Stop()
    {
        _playCts?.Cancel();
        StopPlayer();
        PlaybackState.Set(PlaybackStatus.Idle, null);
    }

    public void Dispose()
    {
        _playCts?.Cancel();
        StopPlayer();
        _device?.Dispose();
    }

    #region Injections

    public required AudioEngine Engine { get; init; }
    public required PlaybackState PlaybackState { get; init; }
    public required ILogger<AudioPlayerService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
