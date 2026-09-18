using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PhotoFileFilter.Review;

// Fixed-size tiles, pixel scrolling and one row of overscan on either side.
public sealed class VirtualizingPhotoPanel : VirtualizingPanel, IScrollInfo
{
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(VirtualizingPhotoPanel), new FrameworkPropertyMetadata(178d, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidSize);
    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(VirtualizingPhotoPanel), new FrameworkPropertyMetadata(157d, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidSize);
    private static bool ValidSize(object value) => value is double size && double.IsFinite(size) && size > 0;
    public double ItemWidth { get => (double)GetValue(ItemWidthProperty); set => SetValue(ItemWidthProperty, value); }
    public double ItemHeight { get => (double)GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }
    public int Columns { get; private set; } = 1;
    public int RealizedCount => InternalChildren.Count;
    private double _offset;

    protected override Size MeasureOverride(Size availableSize)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner == null) return new();
        var width = double.IsFinite(availableSize.Width) ? availableSize.Width : Math.Max(ItemWidth, ActualWidth);
        var height = double.IsFinite(availableSize.Height) ? availableSize.Height : Math.Max(ItemHeight, ActualHeight);
        Columns = Math.Max(1, (int)(width / ItemWidth));
        ViewportWidth = width; ViewportHeight = height;
        ExtentWidth = width; ExtentHeight = Math.Ceiling(owner.Items.Count / (double)Columns) * ItemHeight;
        _offset = Math.Clamp(_offset, 0, Math.Max(0, ExtentHeight - height));
        ScrollOwner?.InvalidateScrollInfo();
        var first = Math.Max(0, (int)(_offset / ItemHeight) - 1) * Columns;
        var last = Math.Min(owner.Items.Count - 1, ((int)((_offset + height) / ItemHeight) + 2) * Columns - 1);
        var children = InternalChildren; // Initialize the items host before requesting its generator.
        var generator = ItemContainerGenerator;
        for (var i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var position = new GeneratorPosition(i, 0);
            var index = generator.IndexFromGeneratorPosition(position);
            if (index >= first && index <= last) continue;
            generator.Remove(position, 1); RemoveInternalChildRange(i, 1);
        }
        if (last >= first)
        {
            var start = generator.GeneratorPositionFromIndex(first);
            var childIndex = start.Offset == 0 ? start.Index : start.Index + 1;
            using (generator.StartAt(start, GeneratorDirection.Forward, true))
            {
                for (var index = first; index <= last; index++, childIndex++)
                {
                    var child = (UIElement)generator.GenerateNext(out var created);
                    if (created)
                    {
                        InsertInternalChild(childIndex, child);
                        generator.PrepareItemContainer(child);
                    }
                    child.Measure(new Size(ItemWidth, ItemHeight));
                }
            }
        }
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (var i = 0; i < InternalChildren.Count; i++)
        {
            var index = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
            InternalChildren[i].Arrange(new Rect(index % Columns * ItemWidth, index / Columns * ItemHeight - _offset, ItemWidth, ItemHeight));
        }
        return finalSize;
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        // Collection resets and removals invalidate generator/container associations.
        if (args.Action is NotifyCollectionChangedAction.Reset or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace or NotifyCollectionChangedAction.Move)
        {
            ItemContainerGenerator.RemoveAll();
            RemoveInternalChildRange(0, InternalChildren.Count);
        }
        InvalidateMeasure();
    }

    protected override void BringIndexIntoView(int index)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner == null || index < 0 || index >= owner.Items.Count) return;
        var top = index / Columns * ItemHeight;
        if (top < _offset) SetVerticalOffset(top);
        else if (top + ItemHeight > _offset + ViewportHeight) SetVerticalOffset(top + ItemHeight - ViewportHeight);
    }
    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner == null) return Rect.Empty;
        var container = ItemsControl.ContainerFromElement(owner, visual);
        if (container != null) BringIndexIntoView(owner.ItemContainerGenerator.IndexFromContainer(container));
        return rectangle;
    }
    public void SetVerticalOffset(double offset)
    {
        if (double.IsNaN(offset)) return;
        var next = Math.Clamp(offset, 0, Math.Max(0, ExtentHeight - ViewportHeight));
        if (Math.Abs(_offset - next) < 0.01) return;
        _offset = next; ScrollOwner?.InvalidateScrollInfo(); InvalidateMeasure();
    }
    public bool CanVerticallyScroll { get; set; }
    public bool CanHorizontallyScroll { get; set; }
    public double ExtentWidth { get; private set; }
    public double ExtentHeight { get; private set; }
    public double ViewportWidth { get; private set; }
    public double ViewportHeight { get; private set; }
    public double VerticalOffset => _offset;
    public double HorizontalOffset => 0;
    public ScrollViewer? ScrollOwner { get; set; }
    public void LineUp() => SetVerticalOffset(_offset - ItemHeight);
    public void LineDown() => SetVerticalOffset(_offset + ItemHeight);
    public void PageUp() => SetVerticalOffset(_offset - ViewportHeight);
    public void PageDown() => SetVerticalOffset(_offset + ViewportHeight);
    public void MouseWheelUp() => SetVerticalOffset(_offset - ItemHeight * 3);
    public void MouseWheelDown() => SetVerticalOffset(_offset + ItemHeight * 3);
    public void SetHorizontalOffset(double offset) { }
    public void LineLeft() { }
    public void LineRight() { }
    public void PageLeft() { }
    public void PageRight() { }
    public void MouseWheelLeft() { }
    public void MouseWheelRight() { }
}
