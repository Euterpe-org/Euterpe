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

    public string? PlayingFilePath { get; private set; }

    public event EventHandler<string?>? PlayingFileChanged;

    public void Play(string filePath)
    {
        StopPlayer();

        try
        {
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
            SetPlayingFile(filePath);

            Logger.ZLogInformation($"Playing audio {filePath}");
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to start audio playback for {filePath}");
            NotificationService.ErrorLight(Notification_Content_Audio_Play_Failed);
            StopPlayer();
            SetPlayingFile(null);
        }
    }

    public void Pause() => _player?.Pause();

    public void Resume() => _player?.Play();

    public void Stop()
    {
        StopPlayer();
        SetPlayingFile(null);
    }

    public void Dispose()
    {
        StopPlayer();
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

    private void StopPlayer()
    {
        if (_player is null)
        {
            return;
        }

        _player.PlaybackEnded -= OnPlayerPlaybackEnded;
        _player.Stop();
        _device?.MasterMixer.RemoveComponent(_player);
        _player.Dispose();
        _player = null;
    }

    private void SetPlayingFile(string? filePath)
    {
        if (PlayingFilePath == filePath)
        {
            return;
        }

        PlayingFilePath = filePath;
        PlayingFileChanged?.Invoke(this, filePath);
    }

    private void OnPlayerPlaybackEnded(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _player))
        {
            SetPlayingFile(null);
        }
    }

    #region Injections

    public required AudioEngine Engine { get; init; }
    public required ILogger<AudioPlayerService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
