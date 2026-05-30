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

    public void Play(string filePath)
    {
        StopInternal();

        var device = EnsureDevice();
        var provider = new StreamDataProvider(Engine, Format, File.OpenRead(filePath));
        _player = new SoundPlayer(Engine, Format, provider);
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

        _player.Stop();
        _device?.MasterMixer.RemoveComponent(_player);
        _player.Dispose();
        _player = null;
    }

    #region Injections

    public required AudioEngine Engine { get; init; }
    public required ILogger<AudioPlayerService> Logger { get; init; }

    #endregion Injections
}