using System.Windows;
using System.Windows.Media;

namespace PhotoFileFilter.Services;

public static class ThemeService
{
    public static void Apply(bool dark)
    {
        if (Application.Current == null) return;
        var palette = new ResourceDictionary { Source = new Uri("/PhotoFileFilter;component/ThemeColors.xaml", UriKind.Relative) };
        foreach (string key in palette.Keys)
        {
            var original = ((SolidColorBrush)palette[key]).Color;
            var hex = $"#{original.R:X2}{original.G:X2}{original.B:X2}";
            var target = !dark ? hex : key switch
            {
                "Surface" => "#252525", "Ink" => "#E4E4E4", "Muted" => "#A8A8A8", "Line" => "#414141", "Accent" => "#2D8FC7", "AccentText" => "#69B8E6",
                _ => DarkColor(hex)
            };
            Application.Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(target));
        }
    }
    private static string DarkColor(string hex) => hex switch
    {
        "#172E38" or "#14232B" => "#151515",
        "#F3F5F3" => "#1B1B1B",
        "#20343F" or "#173D32" or "#172D38" or "#184D3B" => "#DDEBE5",
        "#127C74" => "#2D8FC7",
        "#FBF4E7" => "#3A3123",
        "#A87928" or "#9B5B20" => "#E9BF77",
        "#EAF4ED" or "#E4F1EC" or "#DFEEE7" => "#243B49",
        "#E0E7E4" or "#E2E8E5" or "#DCE5DF" or "#D8E7DF" or "#C4D1CC" => "#3C5359",
        "#247650" or "#216D57" => "#86C8EE",
        "#F2F5F7" => "#303030",
        "#F0F4F3" or "#F8FAF9" or "#FAFBFA" or "#F5F9F6" or "#EAF3F0" or "#EAF3EE" or "#EFF5F0" or "#F5F8F6" or "#EDF3EF" => "#323232",
        "#F0F3F1" => "#383838", "#FBFCFB" => "#292929",
        "#74838B" or "#8B9B92" or "#798B81" => "#A9BBBA",
        "#48756D" or "#3F8A73" or "#5D8673" or "#658170" or "#678078" => "#A0CABB",
        _ => hex
    };
}
