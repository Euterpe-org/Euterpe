using System.Collections;
using System.Collections.Specialized;
using Avalonia.Collections;
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

    private readonly AvaloniaList<WrapRow> _rows = [];
    private bool _attached;
    private int _columns = 1;
    private INotifyCollectionChanged? _subscription;

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

    public IReadOnlyList<WrapRow> Rows => _rows;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            ResubscribeAndRebuild();
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        ResubscribeAndRebuild();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _attached = false;
        Unsubscribe();
    }

    private void ResubscribeAndRebuild()
    {
        Unsubscribe();
        if (_attached && ItemsSource is INotifyCollectionChanged notifier)
        {
            notifier.CollectionChanged += OnSourceCollectionChanged;
            _subscription = notifier;
        }

        RebuildFromSource();
    }

    private void Unsubscribe()
    {
        if (_subscription is null)
        {
            return;
        }

        _subscription.CollectionChanged -= OnSourceCollectionChanged;
        _subscription = null;
    }

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
        RebuildFromSource();
    }

    // Granular edits assume _rows is already chunked at the current _columns; a column change rebuilds first, and
    // CollectionChanged is marshalled onto this UI thread, so a reflow and an incremental edit never interleave.
    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems is { } added && e.NewStartingIndex >= 0:
                for (var i = 0; i < added.Count; i++)
                {
                    InsertItem(e.NewStartingIndex + i, added[i]);
                }

                break;
            case NotifyCollectionChangedAction.Remove when e.OldItems is { } removed && e.OldStartingIndex >= 0:
                for (var i = 0; i < removed.Count; i++)
                {
                    RemoveItem(e.OldStartingIndex);
                }

                break;
            case NotifyCollectionChangedAction.Replace when e.NewItems is { } replaced && e.NewStartingIndex >= 0:
                for (var i = 0; i < replaced.Count; i++)
                {
                    ReplaceItem(e.NewStartingIndex + i, replaced[i]);
                }

                break;
            case NotifyCollectionChangedAction.Move when e.OldItems is { Count: 1 } moved && e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0:
                RemoveItem(e.OldStartingIndex);
                InsertItem(e.NewStartingIndex, moved[0]);
                break;
            default:
                RebuildFromSource();
                break;
        }
    }

    private void RebuildFromSource()
    {
        _rows.Clear();
        _rows.AddRange(BuildRows(ItemsSource, _columns));
    }

    private static List<WrapRow> BuildRows(IEnumerable? source, int columns)
    {
        var rows = new List<WrapRow>();
        if (source is null)
        {
            return rows;
        }

        WrapRow? row = null;
        var column = 0;
        foreach (var item in source)
        {
            if (column == 0)
            {
                row = new WrapRow();
                rows.Add(row);
            }

            row!.Items.Add(item);
            column = (column + 1) % columns;
        }

        return rows;
    }

    private void InsertItem(int index, object? item)
    {
        var rowIndex = index / _columns;
        var offset = index % _columns;
        if (rowIndex > _rows.Count
            || (rowIndex == _rows.Count && offset != 0)
            || (rowIndex < _rows.Count && offset > _rows[rowIndex].Items.Count))
        {
            RebuildFromSource();
            return;
        }

        if (rowIndex == _rows.Count)
        {
            _rows.Add(new WrapRow());
        }

        var carry = item;
        for (var i = rowIndex; i < _rows.Count; i++)
        {
            var items = _rows[i].Items;
            items.Insert(i == rowIndex ? offset : 0, carry);
            if (items.Count <= _columns)
            {
                return;
            }

            carry = items[_columns];
            items.RemoveAt(_columns);
        }

        _rows.Add(new WrapRow());
        _rows[^1].Items.Add(carry);
    }

    private void RemoveItem(int index)
    {
        var rowIndex = index / _columns;
        var offset = index % _columns;
        if (rowIndex >= _rows.Count || offset >= _rows[rowIndex].Items.Count)
        {
            RebuildFromSource();
            return;
        }

        _rows[rowIndex].Items.RemoveAt(offset);
        for (var i = rowIndex; i < _rows.Count - 1; i++)
        {
            var next = _rows[i + 1].Items;
            _rows[i].Items.Add(next[0]);
            next.RemoveAt(0);
        }

        if (_rows[^1].Items.Count == 0)
        {
            _rows.RemoveAt(_rows.Count - 1);
        }
    }

    private void ReplaceItem(int index, object? item)
    {
        var rowIndex = index / _columns;
        var offset = index % _columns;
        if (rowIndex >= _rows.Count || offset >= _rows[rowIndex].Items.Count)
        {
            RebuildFromSource();
            return;
        }

        _rows[rowIndex].Items[offset] = item;
    }
}
