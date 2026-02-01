namespace Euterpe.Tests.UpdateServiceTests;

[Category("UpdateServiceTests")]
[TestSubject(typeof(UpdateService))]
public sealed class UpdateServiceGeneralTests : UpdateServiceTestBase
{
    [Test]
    public async Task CheckForUpdatesAsync_WithInvalidUpdateSource_ThrowsUnreachableException()
    {
        var updateService = CreateUpdateService(
            new Config
            {
                UpdateSource = (UpdateSource)10
            });

        await Assert.ThrowsAsync<UnreachableException>(() => updateService.CheckForUpdatesAsync());
    }
}