using System.Windows;
using System.Windows.Media;

namespace PhotoFileFilter.Shared.Services;

public static class ThemeService
{
    private static readonly IReadOnlyDictionary<string, string> DarkPalette = new Dictionary<string, string>
    {
        ["Accent"] = "#2D8FC7", ["AccentText"] = "#69B8E6", ["AppBackground"] = "#1B1B1B", ["Surface"] = "#252525", ["SurfaceSubtle"] = "#303030", ["SurfaceRaised"] = "#323232",
        ["ButtonBackground"] = "#323232", ["FieldBackground"] = "#323232", ["FieldBorder"] = "#3C5359", ["FieldFocus"] = "#46988D", ["Line"] = "#414141", ["ControlBorder"] = "#3C5359", ["FocusRing"] = "#58A69E",
        ["Ink"] = "#E4E4E4", ["Muted"] = "#A8A8A8", ["QuietText"] = "#A0CABB", ["IconMuted"] = "#A0CABB", ["HeaderBackground"] = "#151515", ["HeaderBadgeBackground"] = "#26443F", ["HeaderMutedText"] = "#ACBFC3", ["HeaderSuccessText"] = "#BDE2CE", ["PreviewCanvas"] = "#151515",
        ["AccentTint"] = "#323232", ["HoverBackground"] = "#323232", ["SelectionBackground"] = "#243B49", ["SelectionText"] = "#DDEBE5", ["SuccessBackground"] = "#243B49", ["SuccessText"] = "#86C8EE", ["SuccessStrong"] = "#86C8EE",
        ["WarningBackground"] = "#3A3123", ["WarningText"] = "#E9BF77", ["WarningStrong"] = "#E9BF77", ["ProgressTrack"] = "#323232", ["StatusText"] = "#A0CABB", ["ReadyText"] = "#A0CABB", ["AccentMuted"] = "#A0CABB",
        ["DropBackground"] = "#323232", ["DropBorder"] = "#3C5359", ["EmptyBackground"] = "#323232", ["EmptyIcon"] = "#8CA999", ["SearchIcon"] = "#A9BBBA", ["TableHeaderBackground"] = "#323232", ["TableHeaderText"] = "#A9BBBA", ["PopupBorder"] = "#3C5359",
        ["ChipBackground"] = "#323232", ["ChipBorder"] = "#3C5359", ["ChipSelectedBackground"] = "#243B49", ["ChipSelectedBorder"] = "#A9D1C2", ["ChipHover"] = "#579B87", ["ChipFocus"] = "#DDEBE5", ["StrongFocus"] = "#DDEBE5"
    };

    public static void Apply(bool dark)
    {
        if (Application.Current == null) return;
        var lightPalette = new ResourceDictionary { Source = new Uri("/PhotoFileFilter;component/Shared/Resources/ThemeColors.xaml", UriKind.Relative) };
        foreach (string key in lightPalette.Keys)
        {
            var light = ((SolidColorBrush)lightPalette[key]).Color;
            var color = dark && DarkPalette.TryGetValue(key, out var darkHex) ? (Color)ColorConverter.ConvertFromString(darkHex) : light;
            Application.Current.Resources[key] = new SolidColorBrush(color);
        }
    }
}
