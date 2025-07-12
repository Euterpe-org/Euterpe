namespace MuseDashModTools.Extensions;

public static class AppBuilderExtensions
{
    public static AppBuilder HandleUIThreadException(this AppBuilder builder, Action<DispatcherUnhandledExceptionEventArgs> action)
    {
        return builder.AfterSetup(_ =>
        {
            Observable.FromEvent<DispatcherUnhandledExceptionEventHandler, DispatcherUnhandledExceptionEventArgs>(
                    handler => (_, args) => handler(args),
                    handler => Dispatcher.UIThread.UnhandledException += handler,
                    handler => Dispatcher.UIThread.UnhandledException -= handler)
                .Subscribe(action);
        });
    }
}