using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhotoFileFilter.Review;

public sealed class RatingBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var rating = value is int number ? number : 0;
        var star = int.TryParse(parameter?.ToString(), out var parsed) ? parsed : 1;
        return rating >= star ? new SolidColorBrush(Color.FromRgb(242, 193, 78)) : new SolidColorBrush(Color.FromRgb(105, 105, 105));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
