using Euterpe.Core.Utils;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Core.Utils;

[Category("DownloadUtilsTests")]
[TestSubject(typeof(DownloadUtils))]
public sealed class DownloadUtilsTest
{
    private const int MaxRetries = 3;
    private const string WrongHash = "0000000000000000000000000000000000000000000000000000000000000000";

    private static readonly byte[] Content = "download-utils-content"u8.ToArray();
    private static readonly string ContentHash = SHA256Utils.HexLowerFromBytes(Content);

    private string _filePath = null!;

    [Before(Test)]
    public void Setup() => _filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [After(Test)]
    public void Cleanup()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    [Test]
    public async Task DownloadVerifiedAsync_NullExpectedHash_AcceptsFirstSuccessfulDownloadWithoutVerifying()
    {
        var calls = 0;
        var download = WriteContentAfter(() => calls++);

        await DownloadUtils.DownloadVerifiedAsync(download, _filePath, null, "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task DownloadVerifiedAsync_EmptyExpectedHash_AcceptsFirstSuccessfulDownloadWithoutVerifying()
    {
        var calls = 0;
        var download = WriteContentAfter(() => calls++);

        await DownloadUtils.DownloadVerifiedAsync(download, _filePath, "", "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task DownloadVerifiedAsync_MatchingHash_Succeeds()
    {
        var calls = 0;
        var download = WriteContentAfter(() => calls++);

        await DownloadUtils.DownloadVerifiedAsync(download, _filePath, ContentHash, "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task DownloadVerifiedAsync_MatchingHashDifferentCase_Succeeds()
    {
        var calls = 0;
        var download = WriteContentAfter(() => calls++);

        await DownloadUtils.DownloadVerifiedAsync(
            download, _filePath, ContentHash.ToUpperInvariant(), "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task DownloadVerifiedAsync_TransientFailureThenSuccess_RetriesAndSucceeds()
    {
        var calls = 0;
        Func<CancellationToken, Task> download = _ =>
        {
            calls++;
            if (calls == 1)
            {
                throw new IOException("transient failure");
            }

            File.WriteAllBytes(_filePath, Content);
            return Task.CompletedTask;
        };

        await DownloadUtils.DownloadVerifiedAsync(download, _filePath, ContentHash, "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(2);
    }

    [Test]
    public async Task DownloadVerifiedAsync_DownloadAlwaysThrows_ThrowsIOExceptionAfterMaxRetries()
    {
        var calls = 0;
        Func<CancellationToken, Task> download = _ =>
        {
            calls++;
            throw new IOException("download failed");
        };

        var act = () => DownloadUtils.DownloadVerifiedAsync(download, _filePath, ContentHash, "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        using var _ = Assert.Multiple();
        await Assert.That(act).Throws<IOException>();
        await Assert.That(calls).IsEqualTo(MaxRetries);
    }

    [Test]
    public async Task DownloadVerifiedAsync_HashAlwaysMismatches_ThrowsIOExceptionAfterMaxRetries()
    {
        var calls = 0;
        var download = WriteContentAfter(() => calls++);

        var act = () => DownloadUtils.DownloadVerifiedAsync(download, _filePath, WrongHash, "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        using var _ = Assert.Multiple();
        await Assert.That(act).Throws<IOException>();
        await Assert.That(calls).IsEqualTo(MaxRetries);
    }

    [Test]
    public async Task DownloadVerifiedAsync_DownloadThrowsOperationCanceled_PropagatesWithoutRetrying()
    {
        var calls = 0;
        Func<CancellationToken, Task> download = _ =>
        {
            calls++;
            throw new OperationCanceledException();
        };

        var act = () => DownloadUtils.DownloadVerifiedAsync(download, _filePath, ContentHash, "asset", MaxRetries, NullLogger.Instance, CancellationToken.None);

        using var _ = Assert.Multiple();
        await Assert.That(act).Throws<OperationCanceledException>();
        await Assert.That(calls).IsEqualTo(1);
    }

    private Func<CancellationToken, Task> WriteContentAfter(Action? onCall = null) =>
        _ =>
        {
            onCall?.Invoke();
            File.WriteAllBytes(_filePath, Content);
            return Task.CompletedTask;
        };
}
