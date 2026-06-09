using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Euterpe.Core;

internal sealed class AudioPlayerService : IAudioPlayerService
{
    private static readonly AudioFormat Format = AudioFormat.DvdHq;

    private AudioPlaybackDevice? _device;
    private SoundPlayer? _player;

    public event EventHandler? PlaybackEnded;

    public void Play(string filePath)
    {
        StopInternal();

        var device = EnsureDevice();
        var provider = new ResilientSoundDataProvider(new StreamDataProvider(Engine, Format, File.OpenRead(filePath)), Logger);
        _player = new SoundPlayer(Engine, Format, provider);
        _player.PlaybackEnded += OnPlayerPlaybackEnded;
        device.MasterMixer.AddComponent(_player);
        _player.Play();

        Logger.ZLogInformation($"Playing audio {filePath}");
    }

    public void Pause() => _player?.Pause();

    public void Resume() => _player?.Play();

    public void Stop() => StopInternal();

    public void Dispose()
    {
        StopInternal();
        _device?.Dispose();
    }

    private AudioPlaybackDevice EnsureDevice()
    {
        if (_device is not null)
        {
            return _device;
        }

        _device = Engine.InitializePlaybackDevice(null, Format);
        _device.Start();
        return _device;
    }

    private void StopInternal()
    {
        if (_player is null)
        {
            return;
        }

        // Detach before stopping so manual stops / track switches don't raise PlaybackEnded;
        // only natural end-of-stream (which fires while still attached) reaches subscribers.
        _player.PlaybackEnded -= OnPlayerPlaybackEnded;
        _player.Stop();
        _device?.MasterMixer.RemoveComponent(_player);
        _player.Dispose();
        _player = null;
    }

    private void OnPlayerPlaybackEnded(object? sender, EventArgs e) => PlaybackEnded?.Invoke(this, e);

    #region Injections

    public required AudioEngine Engine { get; init; }
    public required ILogger<AudioPlayerService> Logger { get; init; }

    #endregion Injections
}
