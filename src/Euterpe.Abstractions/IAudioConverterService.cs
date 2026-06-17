namespace Euterpe.Abstractions;

public interface IAudioConverterService
{
    void Convert(
        string sourcePath,
        string destinationPath,
        string targetFormatId,
        CancellationToken cancellationToken = default);
}
