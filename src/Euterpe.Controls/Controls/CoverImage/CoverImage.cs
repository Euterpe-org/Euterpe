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
        if (change.Property == SourceProperty && _gifPart is not null)
        {
            ApplySource();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_reloadOnAttach && _gifPart is not null)
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

        if (IsAnimated(Source))
        {
            _staticPart.IsVisible = false;
            _gifPart.IsVisible = true;
            _loadCts = new CancellationTokenSource();
            _ = LoadGifAsync(Source!, _loadCts.Token);
            return;
        }

        ShowStatic();
    }

    private void ShowStatic()
    {
        if (_staticPart is null || _gifPart is null)
        {
            return;
        }

        _gifPart.Source = null;
        _gifPart.IsVisible = false;
        DisposeGifSource();
        _staticPart.IsVisible = true;
    }

    private async Task LoadGifAsync(string source, CancellationToken token)
    {
        var built = await BuildGifSourceAsync(source, token).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested || _gifPart is null)
            {
                built?.Dispose();
                return;
            }

            if (built is null)
            {
                ShowStatic();
                return;
            }

            var previous = _gifSource;
            _gifSource = built;
            _gifPart.Source = built;
            previous?.Dispose();
        });
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
