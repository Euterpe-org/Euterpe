using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Windows;

[Category("WindowsGamePathEnvironmentTests")]
[TestSubject(typeof(WindowsGamePathEnvironment))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
[NotInParallel(nameof(WindowsGamePathEnvironmentTest))]
public sealed class WindowsGamePathEnvironmentTest
{
    private const string TestFolder = @"C:\Test\MuseDash";
    private readonly MockLogger<WindowsGamePathEnvironment> _logger = Mock.Logger<WindowsGamePathEnvironment>();
    private string _envName = null!;

    [Before(Test)]
    public void CaptureEnvName() => _envName = new MuseDashConfig().PathEnvironmentVariableName;

    [After(Test)]
    public void ClearEnv()
    {
        // Set() on Windows writes to user scope; clear both to be safe.
        Environment.SetEnvironmentVariable(_envName, null);
        Environment.SetEnvironmentVariable(_envName, null, EnvironmentVariableTarget.User);
    }

    private WindowsGamePathEnvironment CreateService(IMessageBoxService? messageBox = null) => new()
    {
        GameConfig = new MuseDashConfig { Folder = TestFolder },
        Logger = _logger,
        MessageBoxService = messageBox ?? IMessageBoxService.Mock()
    };

    [Test]
    public async Task IsSet_ReflectsEnvironmentVariable()
    {
        var service = CreateService();

        using var _ = Assert.Multiple();

        Environment.SetEnvironmentVariable(_envName, null);
        await Assert.That(service.IsSet()).IsFalse();

        Environment.SetEnvironmentVariable(_envName, TestFolder);
        await Assert.That(service.IsSet()).IsTrue();

        Environment.SetEnvironmentVariable(_envName, @"C:\different\path");
        await Assert.That(service.IsSet()).IsFalse();
    }

    [Test]
    public async Task Set_WritesUserEnvironmentVariableAndReturnsTrue()
    {
        var service = CreateService();
        var result = service.Set();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        await Assert.That(Environment.GetEnvironmentVariable(_envName, EnvironmentVariableTarget.User)).IsEqualTo(TestFolder);
        _logger.VerifyLog().ContainingMessage($"Set {_envName}");
    }
}