using Avalonia.Controls.Metadata;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Euterpe.Controls;

[TemplatePart("PART_Image", typeof(Image))]
[TemplatePart("PART_PlaceholderImage", typeof(Image))]
public sealed class AsyncImage : TemplatedControl
{
    private static readonly HttpClient SharedHttpClient = new();

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<AsyncImage, string?>(nameof(Source));

    public static readonly StyledProperty<IImage?> PlaceholderSourceProperty =
        AvaloniaProperty.Register<AsyncImage, IImage?>(nameof(PlaceholderSource));

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<AsyncImage, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<double> DecodeWidthProperty =
        AvaloniaProperty.Register<AsyncImage, double>(nameof(DecodeWidth), double.NaN);

    public static readonly StyledProperty<double> DecodeHeightProperty =
        AvaloniaProperty.Register<AsyncImage, double>(nameof(DecodeHeight), double.NaN);

    private Bitmap? _currentBitmap;

    private Image? _imagePart;
    private CancellationTokenSource? _loadCts;
    private Image? _placeholderPart;
    private bool _reloadOnAttach;

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public IImage? PlaceholderSource
    {
        get => GetValue(PlaceholderSourceProperty);
        set => SetValue(PlaceholderSourceProperty, value);
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

    public double DecodeHeight
    {
        get => GetValue(DecodeHeightProperty);
        set => SetValue(DecodeHeightProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _imagePart = e.NameScope.Get<Image>("PART_Image");
        _placeholderPart = e.NameScope.Get<Image>("PART_PlaceholderImage");
        _ = LoadAsync(Source);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SourceProperty && _imagePart is not null)
        {
            _ = LoadAsync(Source);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_reloadOnAttach && _imagePart is not null)
        {
            _reloadOnAttach = false;
            _ = LoadAsync(Source);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _loadCts?.Cancel();
        if (_imagePart is not null)
        {
            _imagePart.Source = null;
            _imagePart.Opacity = 0;
        }

        _currentBitmap?.Dispose();
        _currentBitmap = null;
        _reloadOnAttach = true;
    }

    private async Task LoadAsync(string? source)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        if (_imagePart is not null)
        {
            _imagePart.Opacity = 0;
            _imagePart.Source = null;
        }

        if (_placeholderPart is not null)
        {
            _placeholderPart.IsVisible = true;
        }

        if (string.IsNullOrEmpty(source) || !Uri.TryCreate(source, UriKind.Absolute, out var uri))
        {
            return;
        }

        var bitmap = await DecodeAsync(uri, token).ConfigureAwait(false);
        if (bitmap is null)
        {
            return;
        }

        if (token.IsCancellationRequested)
        {
            bitmap.Dispose();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested || _imagePart is null)
            {
                bitmap.Dispose();
                return;
            }

            var previous = _currentBitmap;
            _currentBitmap = bitmap;
            _imagePart.Source = bitmap;
            _imagePart.Opacity = 1;
            if (_placeholderPart is not null)
            {
                _placeholderPart.IsVisible = false;
            }

            previous?.Dispose();
        });
    }

    private async Task<Bitmap?> DecodeAsync(Uri uri, CancellationToken token)
    {
        var decodeWidth = DecodeWidth;
        var decodeHeight = DecodeHeight;
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;

        try
        {
            if (uri.IsFile)
            {
                var localPath = uri.LocalPath;
                return await Task.Run(() =>
                {
                    using var stream = File.OpenRead(localPath);
                    return Decode(stream, decodeWidth, decodeHeight, scaling);
                }, token).ConfigureAwait(false);
            }

            var httpStream = await SharedHttpClient.GetStreamAsync(uri, token).ConfigureAwait(false);
            await using (httpStream.ConfigureAwait(false))
            {
                using var memoryStream = new MemoryStream();
                await httpStream.CopyToAsync(memoryStream, token).ConfigureAwait(false);
                memoryStream.Position = 0;
                return Decode(memoryStream, decodeWidth, decodeHeight, scaling);
            }
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap Decode(Stream stream, double width, double height, double scaling)
    {
        if (!double.IsNaN(height) && height > 0)
        {
            return Bitmap.DecodeToHeight(stream, (int)Math.Ceiling(height * scaling));
        }

        if (!double.IsNaN(width) && width > 0)
        {
            return Bitmap.DecodeToWidth(stream, (int)Math.Ceiling(width * scaling));
        }

        return new Bitmap(stream);
    }
}
