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

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        pathEnvironment.Set().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_PathEnvVariableNotSet_SetsIt()
    {
        var pathEnvironment = IGamePathEnvironment.Mock();
        pathEnvironment.IsSet().Returns(false);
        var step = new EnvVariableStep { PathEnvironment = pathEnvironment };

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        pathEnvironment.Set().WasCalled(Times.Once);
    }
}
