using AsyncAwaitBestPractices;
using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Core;

internal sealed class DialogService : IDialogService
{
    public async Task<T?> ShowDialogAsync<TWindow, TViewModel, T>(TViewModel vm, Window? owner = null)
        where TWindow : Window, new()
        where TViewModel : class, IDialog<T>
    {
        var window = new TWindow { DataContext = vm };

        T? result = default;
        EventHandler<T> handler = (_, value) =>
        {
            result = value;
            window.Close();
        };

        vm.RequestClose += handler;

        var closed = new TaskCompletionSource();
        EventHandler closedHandler = (_, _) => closed.TrySetResult();
        window.Closed += closedHandler;

        try
        {
            owner ??= GetCurrentDesktop().MainWindow;
            if (owner is { IsVisible: true })
            {
                window.Icon ??= owner.Icon;
                window.ShowDialog(owner).SafeFireAndForget();
            }
            else
            {
                window.Show();
            }

            await closed.Task.ConfigureAwait(true);
            return result;
        }
        finally
        {
            vm.RequestClose -= handler;
            window.Closed -= closedHandler;
        }
    }

    public Task<T?> ShowOverlayAsync<TView, TViewModel, T>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext =>
        OverlayDialog.ShowCustomAsync<TView, TViewModel, T>(vm, hostId, options, token);

    public Task<DialogResult> ShowStandardOverlayAsync<TView, TViewModel>(
        TViewModel vm,
        OverlayDialogOptions? options = null,
        string? hostId = null,
        CancellationToken? token = null)
        where TView : Control, new()
        where TViewModel : class, IDialogContext =>
        OverlayDialog.ShowStandardAsync<TView, TViewModel>(vm, hostId, options, token);
}