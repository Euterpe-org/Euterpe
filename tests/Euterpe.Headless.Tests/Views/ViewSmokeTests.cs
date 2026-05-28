using Euterpe.Features.Charting;
using Euterpe.Features.Home;
using Euterpe.Features.Logging;
using Euterpe.Features.Modding;
using Euterpe.Features.Setting;
using Euterpe.Features.Setup;
using Euterpe.Features.Wizard;
using Euterpe.Shell;

namespace Euterpe.Headless.Tests.Views;

/// <summary>
///     Smoke tests that each view's XAML loads into the visual tree without exceptions.
///     Catches XAML typos, missing resources, broken compiled bindings, and theme regressions.
/// </summary>
public sealed class ViewSmokeTests : HeadlessTest
{
    [Test]
    public Task ChartingPage_LoadsIntoVisualTree() => Smoke(() => new ChartingPage());

    [Test]
    public Task HomePage_LoadsIntoVisualTree() => Smoke(() => new HomePage());

    [Test]
    public Task LoggingPage_LoadsIntoVisualTree() => Smoke(() => new LoggingPage());

    [Test]
    public Task ModdingPage_LoadsIntoVisualTree() => Smoke(() => new ModdingPage());

    [Test]
    public Task SettingPage_LoadsIntoVisualTree() => Smoke(() => new SettingPage());

    [Test]
    public Task RepairDialog_LoadsIntoVisualTree() => Smoke(() => new RepairDialog());

    [Test]
    public Task WizardDialog_LoadsIntoVisualTree() => Smoke(() => new WizardDialog());

    [Test]
    public Task ExecutionPage_LoadsIntoVisualTree() => Smoke(() => new ExecutionPage());

    [Test]
    public Task GamePathPage_LoadsIntoVisualTree() => Smoke(() => new GamePathPage());

    [Test]
    public Task RolePage_LoadsIntoVisualTree() => Smoke(() => new RolePage());

    [Test]
    public Task CharterToolkitPanel_LoadsIntoVisualTree() => Smoke(() => new CharterToolkitPanel());

    [Test]
    public Task ChartManagePanel_LoadsIntoVisualTree() => Smoke(() => new ChartManagePanel());

    [Test]
    public Task AppLogPanel_LoadsIntoVisualTree() => Smoke(() => new AppLogPanel());

    [Test]
    public Task MelonLoaderLogPanel_LoadsIntoVisualTree() => Smoke(() => new MelonLoaderLogPanel());

    [Test]
    public Task MelonLoaderPanel_LoadsIntoVisualTree() => Smoke(() => new MelonLoaderPanel());

    [Test]
    public Task ModDevelopPanel_LoadsIntoVisualTree() => Smoke(() => new ModDevelopPanel());

    [Test]
    public Task ModManagePanel_LoadsIntoVisualTree() => Smoke(() => new ModManagePanel());

    [Test]
    public Task AboutPanel_LoadsIntoVisualTree() => Smoke(() => new AboutPanel());

    [Test]
    public Task AdvancedPanel_LoadsIntoVisualTree() => Smoke(() => new AdvancedPanel());

    [Test]
    public Task AppearancePanel_LoadsIntoVisualTree() => Smoke(() => new AppearancePanel());

    [Test]
    public Task DownloadPanel_LoadsIntoVisualTree() => Smoke(() => new DownloadPanel());

    [Test]
    public Task ExperiencePanel_LoadsIntoVisualTree() => Smoke(() => new ExperiencePanel());

    [Test]
    public Task FileManagementPanel_LoadsIntoVisualTree() => Smoke(() => new FileManagementPanel());

    [Test]
    public Task MainWindow_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var window = new MainWindow();
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(window).IsNotNull();
        await Assert.That(window.IsLoaded).IsTrue();
    });

    [Test]
    public Task MainSplashWindow_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var window = new MainSplashWindow { MainWindowFactory = () => new MainWindow() };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(window).IsNotNull();
        await Assert.That(window.IsLoaded).IsTrue();
    });

    [Test]
    public Task CrashWindow_LoadsIntoVisualTree() => RunOnUI(async () =>
    {
        var window = new CrashWindow();
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(window).IsNotNull();
        await Assert.That(window.IsLoaded).IsTrue();
    });

    private static Task Smoke(Func<Control> factory) => RunOnUI(async () =>
    {
        var view = factory();
        var window = new Window { Content = view, Width = 800, Height = 600 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var _ = Assert.Multiple();
        await Assert.That(view).IsNotNull();
        await Assert.That(view.IsLoaded).IsTrue();
    });
}