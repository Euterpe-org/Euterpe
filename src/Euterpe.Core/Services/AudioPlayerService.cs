using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Euterpe.Core;

internal sealed class AudioPlayerService : IAudioPlayerService
{
    private AudioPlaybackDevice? _device;
    private AudioFormat? _deviceFormat;
    private SoundPlayer? _player;

    public event EventHandler? PlaybackEnded;

    public void Play(string filePath)
    {
        StopInternal();

        // Nothing in the SoundFlow pipeline resamples, so device, player and provider must all use the source's native format.
        var source = new StreamDataProvider(Engine, File.OpenRead(filePath));
        var format = new AudioFormat
        {
            Format = source.SampleFormat,
            Channels = source.FormatInfo!.ChannelCount,
            Layout = AudioFormat.GetLayoutFromChannels(source.FormatInfo.ChannelCount),
            SampleRate = source.SampleRate
        };

        var device = EnsureDevice(format);
        _player = new SoundPlayer(Engine, format, new ResilientSoundDataProvider(source, Logger));
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

    private AudioPlaybackDevice EnsureDevice(AudioFormat format)
    {
        if (_device is { } existing && _deviceFormat == format)
        {
            return existing;
        }

        _device?.Dispose();
        _device = Engine.InitializePlaybackDevice(null, format);
        _device.Start();
        _deviceFormat = format;
        return _device;
    }

    private void StopInternal()
    {
        if (_player is null)
        {
            return;
        }

        // Detach first so only natural end-of-stream raises PlaybackEnded.
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
