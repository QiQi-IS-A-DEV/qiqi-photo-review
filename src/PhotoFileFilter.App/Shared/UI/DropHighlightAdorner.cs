using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace PhotoFileFilter.Shared.UI;

public sealed class DropHighlightAdorner : Adorner
{
    public DropHighlightAdorner(UIElement element) : base(element) { IsHitTestVisible = false; }
    protected override void OnRender(DrawingContext context)
    {
        var rect = new Rect(2, 2, Math.Max(0, AdornedElement.RenderSize.Width - 4), Math.Max(0, AdornedElement.RenderSize.Height - 4));
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(25, 154, 132)), 2) { DashStyle = DashStyles.Dash };
        context.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(32, 25, 154, 132)), pen, rect, 14, 14);
    }
}
