using SoundFlow.Abstracts.Devices;
using SoundFlow.Structs;

namespace Euterpe.Core;

internal sealed partial class AudioPlayerService
{
    private AudioPlaybackDevice EnsureDevice(AudioFormat format)
    {
        if (_device is { } existing && existing.Format == format)
        {
            return existing;
        }

        _device?.Dispose();
        _device = Engine.InitializePlaybackDevice(null, format);
        _device.Start();
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
}
