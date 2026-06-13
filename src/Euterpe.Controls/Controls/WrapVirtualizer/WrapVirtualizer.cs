using System.Collections;
using System.Collections.Specialized;
using Avalonia.Controls.Templates;
using Euterpe.Controls.Models;

namespace Euterpe.Controls;

public sealed class WrapVirtualizer : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<WrapVirtualizer, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<WrapVirtualizer, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<WrapVirtualizer, double>(nameof(ItemWidth), 1.0);

    public static readonly DirectProperty<WrapVirtualizer, IReadOnlyList<WrapRow>> RowsProperty =
        AvaloniaProperty.RegisterDirect<WrapVirtualizer, IReadOnlyList<WrapRow>>(nameof(Rows), o => o.Rows);

    private IReadOnlyList<object?> _items = [];
    private IReadOnlyList<WrapRow> _rows = [];
    private int _columns = 1;

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    // Per-column stride in pixels (card width plus its horizontal margin); the column count is the panel width divided by this.
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public IReadOnlyList<WrapRow> Rows
    {
        get => _rows;
        private set => SetAndRaise(RowsProperty, ref _rows, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            OnItemsSourceChanged(change.OldValue as IEnumerable, change.NewValue as IEnumerable);
        }
        else if (change.Property == ItemWidthProperty)
        {
            InvalidateMeasure();
        }
    }

    // Column count comes from the measure constraint, not arranged Bounds, so a non-stretching host cannot lock us to one column.
    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateColumns(availableSize.Width);
        return base.MeasureOverride(availableSize);
    }

    private void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldNotifier)
        {
            oldNotifier.CollectionChanged -= OnSourceCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newNotifier)
        {
            newNotifier.CollectionChanged += OnSourceCollectionChanged;
        }

        Snapshot(newValue);
        RebuildRows();
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Snapshot(ItemsSource);
        RebuildRows();
    }

    private void Snapshot(IEnumerable? source) =>
        _items = source?.Cast<object?>().ToArray() ?? [];

    private void UpdateColumns(double width)
    {
        if (double.IsInfinity(width) || width <= 0 || ItemWidth <= 0)
        {
            return;
        }

        var columns = Math.Max(1, (int)(width / ItemWidth));
        if (columns == _columns)
        {
            return;
        }

        _columns = columns;
        RebuildRows();
    }

    private void RebuildRows()
    {
        if (_items.Count is 0 || _columns <= 0)
        {
            Rows = [];
            return;
        }

        var rows = new List<WrapRow>((_items.Count + _columns - 1) / _columns);
        for (var start = 0; start < _items.Count; start += _columns)
        {
            var count = Math.Min(_columns, _items.Count - start);
            var slice = new object?[count];
            for (var offset = 0; offset < count; offset++)
            {
                slice[offset] = _items[start + offset];
            }

            rows.Add(new WrapRow(slice));
        }

        Rows = rows;
    }
}
