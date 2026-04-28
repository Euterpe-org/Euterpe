namespace Euterpe.Tests;

[Category("EnvVariableStepTests")]
[TestSubject(typeof(EnvVariableStep))]
public sealed class EnvVariableStepTest
{
    [Test]
    public async Task ExecuteAsync_PathEnvVariableAlreadySet_DoesNotSetIt()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckPathEnvironmentVariableSet().Returns(true);
        var step = new EnvVariableStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.SetPathEnvironmentVariable().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_PathEnvVariableNotSet_SetsIt()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckPathEnvironmentVariableSet().Returns(false);
        var step = new EnvVariableStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.SetPathEnvironmentVariable().WasCalled(Times.Once);
    }
}