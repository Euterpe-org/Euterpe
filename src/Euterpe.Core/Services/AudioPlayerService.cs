using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Codecs.FFMpeg;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Euterpe.Core;

internal sealed class AudioPlayerService : IAudioPlayerService
{
    private static readonly AudioFormat Format = AudioFormat.DvdHq;
    private readonly AudioEngine _engine = new MiniAudioEngine();

    private AudioPlaybackDevice? _device;
    private SoundPlayer? _player;

    #region Injections

    public required ILogger<AudioPlayerService> Logger { get; init; }

    #endregion Injections

    public AudioPlayerService()
    {
        _engine.RegisterCodecFactory(new FFmpegCodecFactory());
    }

    public void Play(string filePath)
    {
        StopInternal();

        var device = EnsureDevice();
        var provider = new StreamDataProvider(_engine, Format, File.OpenRead(filePath));
        _player = new SoundPlayer(_engine, Format, provider);
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
        _engine.Dispose();
    }

    private AudioPlaybackDevice EnsureDevice()
    {
        if (_device is not null)
        {
            return _device;
        }

        _device = _engine.InitializePlaybackDevice(null, Format);
        _device.Start();
        return _device;
    }

    private void StopInternal()
    {
        if (_player is null)
        {
            return;
        }

        _player.Stop();
        _device?.MasterMixer.RemoveComponent(_player);
        _player.Dispose();
        _player = null;
    }
}