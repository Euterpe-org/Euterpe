using Avalonia.Threading;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Components;
using SoundFlow.Interfaces;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Euterpe.Core;

internal sealed partial class AudioPlayerService
{
    private (ISoundDataProvider Source, AudioFormat Format) Prepare(string filePath)
    {
        var stream = new StreamDataProvider(Engine, File.OpenRead(filePath));
        var format = new AudioFormat
        {
            Format = stream.SampleFormat,
            Channels = stream.FormatInfo!.ChannelCount,
            Layout = AudioFormat.GetLayoutFromChannels(stream.FormatInfo.ChannelCount),
            SampleRate = stream.SampleRate
        };

        return (new ResilientSoundDataProvider(stream, Logger), format);
    }

    // Runs on the UI thread; cts is this play's token, so a play superseded mid-load is discarded.
    private void Activate(ISoundDataProvider source, AudioFormat format, CancellationTokenSource cts)
    {
        if (cts.IsCancellationRequested)
        {
            source.Dispose();
            return;
        }

        var device = EnsureDevice(format);
        var player = new SoundPlayer(Engine, format, source);

        player.PlaybackEnded += OnPlayerPlaybackEnded;
        device.MasterMixer.AddComponent(player);
        _player = player;
        _playerEndedHandler = OnPlayerPlaybackEnded;

        if (PlaybackState.Status is PlaybackStatus.Playing)
        {
            player.Play();
        }

        Logger.LogInformation($"Playing audio {PlaybackState.PlayingKey}");
        return;

        // PlaybackEnded fires on the native audio render thread, so only marshal back and compare identity.
        void OnPlayerPlaybackEnded(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_playCts == cts)
                {
                    Stop();
                }
            });
        }
    }

    private void Fail(CancellationTokenSource cts)
    {
        if (cts.IsCancellationRequested)
        {
            return;
        }

        PlaybackState.Set(PlaybackStatus.Idle, null);
        NotificationService.ErrorLight(Notification_Content_Audio_Play_Failed);
    }

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

        if (_playerEndedHandler is not null)
        {
            _player.PlaybackEnded -= _playerEndedHandler;
        }

        _player.Stop();
        _device?.MasterMixer.RemoveComponent(_player);
        _player.Dispose();
        _player = null;
        _playerEndedHandler = null;
    }
}
