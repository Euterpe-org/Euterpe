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

    private Image? _imagePart;
    private CancellationTokenSource? _loadCts;

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _imagePart = e.NameScope.Get<Image>("PART_Image");
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

        if (string.IsNullOrEmpty(source) || !Uri.TryCreate(source, UriKind.Absolute, out var uri))
        {
            return;
        }

        Bitmap bitmap;
        try
        {
            if (uri.IsFile)
            {
                bitmap = await Task.Run(() => new Bitmap(uri.LocalPath), token).ConfigureAwait(false);
            }
            else
            {
                var stream = await SharedHttpClient.GetStreamAsync(uri, token).ConfigureAwait(false);
                await using (stream.ConfigureAwait(false))
                {
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream, token).ConfigureAwait(false);
                    memoryStream.Position = 0;
                    bitmap = new Bitmap(memoryStream);
                }
            }
        }
        catch
        {
            return;
        }

        if (token.IsCancellationRequested)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested || _imagePart is null)
            {
                return;
            }

            _imagePart.Source = bitmap;
            _imagePart.Opacity = 1;
        });
    }
}