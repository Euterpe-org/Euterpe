using Avalonia.Interactivity;

namespace Euterpe.Controls;

/// <summary>
///     Attached behavior that adds the <c>DragOver</c> style class to a control while files are dragged over it,
///     so an overlay can be revealed purely through a style selector. The drop itself is handled separately
///     (e.g. FilesDropBehavior); this never touches the control's content.
///     An enter/leave depth counter is used because <see cref="DragDrop.AllowDropProperty" /> inherits to
///     children, so the drag events fire repeatedly as the pointer crosses child controls.
/// </summary>
public sealed class DropZone : AvaloniaObject
{
    private const string DragOverClass = "DragOver";

    public static readonly AttachedProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<DropZone, Control, bool>("IsActive");

    private static readonly AttachedProperty<int> DragDepthProperty =
        AvaloniaProperty.RegisterAttached<DropZone, Control, int>("DragDepth");

    static DropZone() => IsActiveProperty.Changed.AddClassHandler<Control>(OnIsActiveChanged);

    private DropZone()
    {
    }

    public static bool GetIsActive(Control control) => control.GetValue(IsActiveProperty);

    public static void SetIsActive(Control control, bool value) => control.SetValue(IsActiveProperty, value);

    private static void OnIsActiveChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.GetNewValue<bool>())
        {
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragEnterEvent, OnDragEnter, RoutingStrategies.Bubble, true);
            control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave, RoutingStrategies.Bubble, true);
            control.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble, true);
        }
        else
        {
            control.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
            control.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            control.RemoveHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    private static void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var depth = control.GetValue(DragDepthProperty) + 1;
        control.SetValue(DragDepthProperty, depth);
        if (depth is 1)
        {
            control.Classes.Add(DragOverClass);
        }
    }

    private static void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        var depth = control.GetValue(DragDepthProperty) - 1;
        if (depth > 0)
        {
            control.SetValue(DragDepthProperty, depth);
            return;
        }

        control.SetValue(DragDepthProperty, 0);
        control.Classes.Remove(DragOverClass);
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        control.SetValue(DragDepthProperty, 0);
        control.Classes.Remove(DragOverClass);
    }
}
