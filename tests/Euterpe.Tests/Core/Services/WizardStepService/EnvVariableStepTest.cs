namespace Euterpe.Tests;

[Category("EnvVariableStepTests")]
[TestSubject(typeof(EnvVariableStep))]
public sealed class EnvVariableStepTest
{
    [Test]
    public async Task ExecuteAsync_PathEnvVariableAlreadySet_DoesNotSetIt()
    {
        var pathEnvironment = IGamePathEnvironment.Mock();
        pathEnvironment.IsSet().Returns(true);
        var step = new EnvVariableStep { PathEnvironment = pathEnvironment };

        await step.ExecuteAsync();

        pathEnvironment.Set().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_PathEnvVariableNotSet_SetsIt()
    {
        var pathEnvironment = IGamePathEnvironment.Mock();
        pathEnvironment.IsSet().Returns(false);
        var step = new EnvVariableStep { PathEnvironment = pathEnvironment };

        await step.ExecuteAsync();

        pathEnvironment.Set().WasCalled(Times.Once);
    }
}