using Avalonia.Controls.Metadata;
using Avalonia.Labs.Gif;
using Avalonia.Threading;

namespace Euterpe.Controls;

[TemplatePart("PART_Static", typeof(AsyncImage))]
[TemplatePart("PART_Gif", typeof(GifImage))]
public sealed class CoverImage : TemplatedControl
{
    private const string AnimatedExtension = ".gif";

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<CoverImage, string?>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<CoverImage, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<double> DecodeWidthProperty =
        AvaloniaProperty.Register<CoverImage, double>(nameof(DecodeWidth), double.NaN);

    private AsyncImage? _staticPart;
    private GifImage? _gifPart;
    private GifStreamSource? _gifSource;
    private CancellationTokenSource? _loadCts;
    private bool _reloadOnAttach;

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public double DecodeWidth
    {
        get => GetValue(DecodeWidthProperty);
        set => SetValue(DecodeWidthProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _staticPart = e.NameScope.Get<AsyncImage>("PART_Static");
        _gifPart = e.NameScope.Get<GifImage>("PART_Gif");
        ApplySource();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty && _staticPart is not null)
        {
            ApplySource();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_reloadOnAttach && _staticPart is not null)
        {
            _reloadOnAttach = false;
            ApplySource();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _loadCts?.Cancel();
        if (_gifPart is not null)
        {
            _gifPart.Source = null;
        }

        if (_staticPart is not null)
        {
            _staticPart.Source = null;
        }

        DisposeGifSource();
        _reloadOnAttach = true;
    }

    private void ApplySource()
    {
        _loadCts?.Cancel();
        if (_staticPart is null || _gifPart is null)
        {
            return;
        }

        var source = Source;
        if (IsAnimated(source))
        {
            _staticPart.Source = null;
            _staticPart.IsVisible = false;
            _gifPart.IsVisible = true;
            _loadCts = new CancellationTokenSource();
            _ = LoadGifAsync(source!, _loadCts.Token);
            return;
        }

        ShowStatic(source);
    }

    private async Task LoadGifAsync(string source, CancellationToken token)
    {
        var built = await BuildGifSourceAsync(source, token).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested || _staticPart is null || _gifPart is null)
            {
                built?.Dispose();
                return;
            }

            if (built is null)
            {
                ShowStatic(source);
                return;
            }

            var previous = _gifSource;
            _gifSource = built;
            _gifPart.Source = built;
            previous?.Dispose();
        });
    }

    private void ShowStatic(string? source)
    {
        if (_staticPart is null || _gifPart is null)
        {
            return;
        }

        _gifPart.Source = null;
        _gifPart.IsVisible = false;
        DisposeGifSource();
        _staticPart.Source = source;
        _staticPart.IsVisible = true;
    }

    private static async Task<GifStreamSource?> BuildGifSourceAsync(string source, CancellationToken token)
    {
        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri) || !uri.IsFile)
        {
            return null;
        }

        try
        {
            return await Task.Run(() =>
            {
                var bytes = File.ReadAllBytes(uri.LocalPath);
                token.ThrowIfCancellationRequested();
                return GifStreamSource.FromStream(new MemoryStream(bytes, writable: false));
            }, token).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    private void DisposeGifSource()
    {
        _gifSource?.Dispose();
        _gifSource = null;
    }

    private static bool IsAnimated(string? source) =>
        source is { } value && value.EndsWith(AnimatedExtension, StringComparison.OrdinalIgnoreCase);
}
