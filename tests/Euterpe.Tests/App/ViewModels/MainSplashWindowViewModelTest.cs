using System.Runtime.CompilerServices;
using DotNext.Threading;
using Euterpe.Shell;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("MainSplashWindowViewModelTests")]
[TestSubject(typeof(MainSplashWindowViewModel))]
public sealed class MainSplashWindowViewModelTest
{
    [Test]
    public async Task OnInitializeAsync_RestoreSessionSucceeds_DoesNotCallLogin()
    {
        var auth = NewAuthService(true, out var loginCount);
        var vm = NewViewModel(auth);

        await vm.InitializeAsync();

        await Assert.That(loginCount.Value).IsEqualTo(0);
    }

    [Test]
    public async Task OnInitializeAsync_RestoreSessionFails_CallsLogin()
    {
        var auth = NewAuthService(false, out var loginCount);
        var vm = NewViewModel(auth);

        await vm.InitializeAsync();

        await Assert.That(loginCount.Value).IsEqualTo(1);
    }

    [Test]
    public async Task OnInitializeAsync_SignalsReady_AndRaisesRequestClose()
    {
        var auth = NewAuthService(true, out _);
        var vm = NewViewModel(auth);
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        await vm.InitializeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(vm.Ready.IsSet).IsTrue();
        await Assert.That(closed).IsTrue();
    }

    [Test]
    public async Task Close_RaisesRequestClose()
    {
        var vm = NewViewModel(NewAuthService(true, out _));
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;

        vm.Close();

        await Assert.That(closed).IsTrue();
    }

    private static IAuthService NewAuthService(bool restoreSucceeds, out StrongBox<int> loginCount)
    {
        var counter = new StrongBox<int>(0);
        loginCount = counter;
        var auth = IAuthService.Mock();
        auth.Ready.Returns(new AsyncManualResetEvent(true));
        auth.RestoreSessionAsync().Returns(restoreSucceeds);
        auth.LoginAsync().Callback(() => counter.Value++);
        return auth;
    }

    private static MainSplashWindowViewModel NewViewModel(IAuthService authService) => new()
    {
        Launcher = IPlatformLauncher.Mock(),
        Logger = NullLogger<MainSplashWindowViewModel>.Instance,
        AuthService = authService,
#if RELEASE
        UpdateService = IUpdateService.Mock()
#endif
    };
}