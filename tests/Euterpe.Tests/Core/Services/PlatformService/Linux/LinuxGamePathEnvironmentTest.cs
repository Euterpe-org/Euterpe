using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Linux;

[Category("LinuxGamePathEnvironmentTests")]
[TestSubject(typeof(LinuxGamePathEnvironment))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxGamePathEnvironmentTest
{
    private const string TestFolder = "/tmp/test-musedash-folder";
    private readonly MockLogger<LinuxGamePathEnvironment> _logger = Mock.Logger<LinuxGamePathEnvironment>();
    private string _envName = null!;

    [Before(Test)]
    public void CaptureEnvName() => _envName = new MuseDashConfig().PathEnvironmentVariableName;

    [After(Test)]
    public void ClearEnv() => Environment.SetEnvironmentVariable(_envName, null);

    private LinuxGamePathEnvironment CreateService(IMessageBoxService? messageBox = null) => new()
    {
        GameConfig = new MuseDashConfig { Folder = TestFolder },
        Logger = _logger,
        MessageBoxService = messageBox ?? IMessageBoxService.Mock()
    };

    [Test]
    public async Task IsSet_ReflectsEnvironmentVariable()
    {
        // Combined into one test because all three branches mutate the same process-wide env var.
        // Splitting them would force [NotInParallel] and lose parallelism.
        var service = CreateService();

        using var _ = Assert.Multiple();

        Environment.SetEnvironmentVariable(_envName, null);
        await Assert.That(service.IsSet()).IsFalse();

        Environment.SetEnvironmentVariable(_envName, TestFolder);
        await Assert.That(service.IsSet()).IsTrue();

        Environment.SetEnvironmentVariable(_envName, "/some/other/path");
        await Assert.That(service.IsSet()).IsFalse();
    }

    [Test]
    public async Task Set_LogsAndReturnsTrue()
    {
        var service = CreateService();
        var result = service.Set();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        _logger.VerifyLog().ContainingMessage($"Ask user to set {_envName}");
    }
}