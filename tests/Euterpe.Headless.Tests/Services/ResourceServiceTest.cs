using Avalonia.Media.Imaging;
using Euterpe.Abstractions;
using Euterpe.Core;

namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(ResourceService))]
public sealed class ResourceServiceTest : HeadlessTest
{
    private static IResourceService NewService() => new ResourceService();

    [Test]
    public Task GetAssetAsStream_ExistingAsset_ReturnsReadableStream() => RunOnUI(async () =>
    {
        var service = NewService();

        await using var stream = service.GetAssetAsStream("Icon.png");

        using var _ = Assert.Multiple();
        await Assert.That(stream).IsNotNull();
        await Assert.That(stream.CanRead).IsTrue();
        await Assert.That(stream.Length).IsGreaterThan(0);
    });

    [Test]
    public Task GetAssetAsStream_MissingAsset_Throws() => RunOnUI(async () =>
    {
        var service = NewService();

        var act = () => service.GetAssetAsStream("__does_not_exist__.png");
        await Assert.That(act).Throws<Exception>();
    });

    [Test]
    public Task TryGetAppResource_AppResourcesKey_ReturnsResource() => RunOnUI(async () =>
    {
        var service = NewService();
        Application.Current!.Resources["TestProbeKey"] = "test-value";
        try
        {
            var value = service.TryGetAppResource<string>("TestProbeKey");

            await Assert.That(value).IsEqualTo("test-value");
        }
        finally
        {
            Application.Current.Resources.Remove("TestProbeKey");
        }
    });

    [Test]
    public Task TryGetAppResource_UnknownKey_ReturnsNull() => RunOnUI(async () =>
    {
        var service = NewService();

        var result = service.TryGetAppResource<object>("__no_such_resource_key__");

        await Assert.That(result).IsNull();
    });

    [Test]
    public Task TryGetAppResource_KeyExistsWrongType_ReturnsNull() => RunOnUI(async () =>
    {
        var service = NewService();
        Application.Current!.Resources["WrongTypeProbe"] = 42;
        try
        {
            // Stored as int, asked as Bitmap → `as Bitmap` returns null.
            var result = service.TryGetAppResource<Bitmap>("WrongTypeProbe");

            await Assert.That(result).IsNull();
        }
        finally
        {
            Application.Current.Resources.Remove("WrongTypeProbe");
        }
    });
}
