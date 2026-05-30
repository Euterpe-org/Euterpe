using SoundFlow.Abstracts;

namespace Euterpe.Core;

internal sealed class AudioConverterService : IAudioConverterService
{
    private const int BufferSize = 8192;

    public void Convert(
        string sourcePath,
        string destinationPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        using var input = File.OpenRead(sourcePath);
        using var decoder = Engine.CreateDecoder(input, out var sourceFormat);
        using var output = File.Create(destinationPath);
        using var encoder = Engine.CreateEncoder(output, targetFormatId, sourceFormat);

        var buffer = new float[BufferSize];
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var read = decoder.Decode(buffer);
            if (read <= 0)
            {
                break;
            }

            encoder.Encode(buffer.AsSpan(0, read));
        }

        Logger.ZLogInformation($"Converted {sourcePath} to {destinationPath} ({targetFormatId})");
    }

    #region Injections

    public required AudioEngine Engine { get; init; }
    public required ILogger<AudioConverterService> Logger { get; init; }

    #endregion Injections
}