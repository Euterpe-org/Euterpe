using Euterpe.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

/// <summary>
///     Routes that don't prefix-match any RouteTree node skip Select-callbacks (which would otherwise
///     hit <c>IocContainer.Resolve</c> and require a bootstrapped scope). All tests use unmatched
///     synthetic routes so we exercise NavigationService's own state logic without app DI.
/// </summary>
[Category("NavigationServiceTests")]
[TestSubject(typeof(NavigationService))]
public sealed class NavigationServiceTest
{
    private const string RouteA = "/__nav_test_a__";
    private const string RouteB = "/__nav_test_b__";

    private static NavigationService NewService() => new()
    {
        Logger = NullLogger<NavigationService>.Instance
    };

    [Test]
    public async Task Ctor_InitialState()
    {
        var service = NewService();

        using var _ = Assert.Multiple();
        await Assert.That(service.CurrentRoute).IsNull();
        await Assert.That(service.Ready.IsSet).IsFalse();
    }

    [Test]
    public async Task NavigateTo_NewRoute_UpdatesCurrentRoute()
    {
        var service = NewService();

        service.NavigateTo(RouteA);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteA);
    }

    [Test]
    public async Task NavigateTo_SameRoute_DoesNothing()
    {
        var service = NewService();
        service.NavigateTo(RouteA);

        service.NavigateTo(RouteA);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteA);
    }

    [Test]
    public async Task NavigateTo_DifferentRoute_OverwritesCurrentRoute()
    {
        var service = NewService();
        service.NavigateTo(RouteA);

        service.NavigateTo(RouteB);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteB);
    }

    [Test]
    public async Task NavigateToAsync_BlocksUntilReadyThenNavigates()
    {
        var service = NewService();
        var task = service.NavigateToAsync(RouteA);

        await Assert.That(task.IsCompleted).IsFalse();

        service.Ready.Set();
        await task;

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteA);
    }

    [Test]
    public async Task NavigateToAsync_ReadyAlreadySet_NavigatesImmediately()
    {
        var service = NewService();
        service.Ready.Set();

        await service.NavigateToAsync(RouteB);

        await Assert.That(service.CurrentRoute).IsEqualTo(RouteB);
    }
}